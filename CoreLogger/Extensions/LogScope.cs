using System.Collections.Immutable;

namespace CoreLogger.Extensions
{
    public static class LogScope
    {
        private static readonly AsyncLocal<IImmutableDictionary<string, object>> _scope = new();
        public static IDisposable BeginScope<TState>(TState state) where TState : notnull
        {
            if (state == null) return NullScope.Instance;
            var dict = new Dictionary<string, object>();

            switch (state)
            {
                case IDictionary<string, object> dic:
                    foreach (var kv in dic) dict[kv.Key] = kv.Value;
                    break;
                case not string when state.GetType().IsClass:
                    foreach (var prop in state.GetType().GetProperties())
                        dict[prop.Name] = prop.GetValue(state)!;
                    break;
                default:
                    dict["Scope"] = state.ToString()!;
                    break;
            }

            var previousScope = _scope.Value;
            _scope.Value = previousScope?.Concat(dict)?.ToImmutableDictionary() ?? dict.ToImmutableDictionary();
            if(previousScope == null)
                return NullScope.Instance;
            return new ScopeDisposable(previousScope);
        }

        internal static IImmutableDictionary<string, object> Current => _scope.Value ?? ImmutableDictionary<string, object>.Empty;

        private sealed class ScopeDisposable(IImmutableDictionary<string, object> previousScope) : IDisposable
        {
            private readonly IImmutableDictionary<string, object> _previousScope = previousScope;
            private bool _disposed;

            public void Dispose()
            {
                if (_disposed) return;
                _scope.Value = _previousScope;
                _disposed = true;
            }
        }

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();
            public void Dispose() { }
        }
    }
}
