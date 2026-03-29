namespace MathEditor.Models;

public class VariableStore
{
    private readonly Dictionary<string, (Guid OwnerId, string Value)> _variables = new();

    public event Action? OnVariablesChanged;


    /// <summary>
    /// Tries to declare or overwrite a stored variable. Returns with a warning message if another
    /// field owns it.
    /// </summary>
    /// <param name="ownerId">ID of the field that declares the variable</param>
    /// <param name="name">Name of the variable</param>
    /// <param name="value">Value of the variable</param>
    /// <param name="warning">The thrown warning message</param>
    /// <returns>Whether the declaration was successful or not</returns>
    public bool Declare(Guid ownerId, string name, string value, out string? warning)
    {
        warning = null;

        if (_variables.TryGetValue(name, out var existing))
        {
            if (existing.OwnerId != ownerId)
                warning = $"Warning: '{name}' was already declared by another field and has been overwritten";
        }

        _variables[name] = (ownerId, value);
        OnVariablesChanged?.Invoke();
        return true;
    }


    /// <summary>
    /// Called when a field stops editing. Removes any variables previously owned
    /// by this field that are no longer declared in its current latex.
    /// </summary>
    /// <param name="ownerId">ID of the field that declared the variable</param>
    /// <param name="currentlyDeclaredNames">List of all currently declared variable names</param>
    public void Reconcile(Guid ownerId, IEnumerable<string> currentlyDeclaredNames)
    {
        var declared = new HashSet<string>(currentlyDeclaredNames);

        var toRemove = _variables
            .Where(kv => kv.Value.OwnerId == ownerId && !declared.Contains(kv.Key))
            .Select(kv => kv.Key)
            .ToList();

        foreach (var key in toRemove)
            _variables.Remove(key);
        
        if (toRemove.Count > 0)
            OnVariablesChanged?.Invoke();
    }

    
    /// <summary>
    /// Tries to get the value associated with the variable name
    /// </summary>
    /// <param name="name">Name of the declared variable</param>
    /// <param name="value">Stored value of the declared variable</param>
    /// <returns></returns>
    public bool TryGet(string name, out string value)
    {
        if (_variables.TryGetValue(name, out var entry))
        {
            value = entry.Value;
            return true;
        }

        value = string.Empty;
        return false;
    }


    public IReadOnlyDictionary<string, string> GetAll() =>
        _variables.ToDictionary(kv => kv.Key, kv => kv.Value.Value);
}