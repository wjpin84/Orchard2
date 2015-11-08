/*
The MIT License (MIT)

Copyright (c) 2014 Maksim Volkau

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/

namespace DryIocExtension {
    using System;
    using System.Threading;
    using DryIoc;
    using System.Collections.Generic;
#if DNX451
    using System.Runtime.Remoting.Messaging;
#endif

    /// <summary>Stores scopes propagating through async-await boundaries.</summary>
    public sealed class AsyncExecutionFlowScopeContext : IScopeContext, IDisposable {
        /// <summary>Statically known name of root scope in this context.</summary>
        public static readonly string ScopeContextName = typeof(AsyncExecutionFlowScopeContext).FullName;

        /// <summary>Name associated with context root scope - so the reuse may find scope context.</summary>
        public string RootScopeName { get { return ScopeContextName; } }

        /// <summary>Creates new scope context.</summary>
        public AsyncExecutionFlowScopeContext() {
            _currentScopeEntryKey = RootScopeName + Interlocked.Increment(ref _seedKey);
        }

        /// <summary>Returns current scope or null if no ambient scope available at the moment.</summary>
        /// <returns>Current scope or null.</returns>
        public IScope GetCurrentOrDefault() {
            var contextState = new ContextState<ScopeEntry<IScope>>(_currentScopeEntryKey);
            var scopeEntry = contextState.GetState();
            return scopeEntry == null ? null : scopeEntry.Value;
        }

        /// <summary>Changes current scope using provided delegate. Delegate receives current scope as input and  should return new current scope.</summary>
        /// <param name="setCurrentScope">Delegate to change the scope.</param>
        /// <remarks>Important: <paramref name="setCurrentScope"/> may be called multiple times in concurrent environment.
        /// Make it predictable by removing any side effects.</remarks>
        /// <returns>New current scope. So it is convenient to use method in "using (var newScope = ctx.SetCurrent(...))".</returns>
        public IScope SetCurrent(SetCurrentScopeHandler setCurrentScope) {
            var oldScope = GetCurrentOrDefault();
            var newScope = setCurrentScope(oldScope);
            var scopeEntry = newScope == null ? null : new ScopeEntry<IScope>(newScope);
            var contextState = new ContextState<ScopeEntry<IScope>>(_currentScopeEntryKey);
            contextState.SetState(scopeEntry);
            return newScope;
        }

        /// <summary>Nothing to dispose.</summary>
        public void Dispose() { }

        private static int _seedKey;
        private readonly string _currentScopeEntryKey;
    }

#if DNX451
    internal sealed class ScopeEntry<T> : MarshalByRefObject {
#else
    internal sealed class ScopeEntry<T> {
#endif
        public readonly T Value;
        public ScopeEntry(T value) { Value = value; }
    }
    
    /// <summary>
    /// Holds some state for the current HttpContext or thread
    /// </summary>
    /// <typeparam name="T">The type of data to store</typeparam>
    public class ContextState<T> where T : class {
        private readonly string _name;
        private readonly Func<T> _defaultValue;

        public ContextState(string name) {
            _name = name;
        }

        public ContextState(string name, Func<T> defaultValue) {
            _name = name;
            _defaultValue = defaultValue;
        }

#if DNX451
        public T GetState() {
            var data = CallContext.GetData(_name);

            if (data == null) {
                if (_defaultValue != null) {
                    CallContext.SetData(_name, data = _defaultValue());
                    return data as T;
                }
            }

            return data as T;
        }

        public void SetState(T state) {
            CallContext.SetData(_name, state);
        }
#else

        private readonly AsyncLocal<IDictionary<string, T>> _serviceProvider = new AsyncLocal<IDictionary<string, T>>();

        public T GetState() {
            if (_serviceProvider.Value.ContainsKey(_name)) {
                return _serviceProvider.Value[_name];
            }

            if (_defaultValue != null) {
                _serviceProvider.Value.Add(_name, _defaultValue());
                return _serviceProvider.Value[_name];
            }

            return default(T);
        }

        public void SetState(T state) {
            _serviceProvider.Value.Add(_name, state);
        }
#endif
    }
}