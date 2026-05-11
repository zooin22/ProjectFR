namespace ProjectFR.Systems;

public enum StatusEffect
{
    Quarantine,
    Compressed,
    Corrupted
}

public class StatusEffectInstance
{
    public StatusEffect Type { get; set; }
    public int Duration { get; set; }
    public int Magnitude { get; set; }

    public StatusEffectInstance(StatusEffect type, int duration, int magnitude = 0)
    {
        Type = type;
        Duration = duration;
        Magnitude = magnitude;
    }

    public void DecrementDuration()
    {
        Duration = System.Math.Max(0, Duration - 1);
    }

    public bool IsExpired => Duration <= 0;
}

public class StatusEffectSystem
{
    private Dictionary<string, List<StatusEffectInstance>> _actorEffects = new();

    public void AddEffect(string actorId, StatusEffect type, int duration, int magnitude = 0)
    {
        if (!_actorEffects.ContainsKey(actorId))
        {
            _actorEffects[actorId] = new();
        }

        _actorEffects[actorId].Add(new StatusEffectInstance(type, duration, magnitude));
    }

    public void RemoveEffect(string actorId, StatusEffect type)
    {
        if (_actorEffects.ContainsKey(actorId))
        {
            _actorEffects[actorId].RemoveAll(e => e.Type == type);
        }
    }

    public bool HasEffect(string actorId, StatusEffect type)
    {
        return _actorEffects.ContainsKey(actorId) &&
               _actorEffects[actorId].Any(e => e.Type == type && !e.IsExpired);
    }

    public List<StatusEffectInstance> GetEffects(string actorId)
    {
        if (!_actorEffects.ContainsKey(actorId))
        {
            return new();
        }
        return _actorEffects[actorId].Where(e => !e.IsExpired).ToList();
    }

    public void UpdateDurations(string actorId)
    {
        if (!_actorEffects.ContainsKey(actorId))
            return;

        foreach (var effect in _actorEffects[actorId])
        {
            effect.DecrementDuration();
        }

        _actorEffects[actorId].RemoveAll(e => e.IsExpired);
    }

    public void ClearEffects(string actorId)
    {
        if (_actorEffects.ContainsKey(actorId))
        {
            _actorEffects[actorId].Clear();
        }
    }

    public int GetAttackModifier(string actorId)
    {
        if (!HasEffect(actorId, StatusEffect.Compressed))
            return 0;

        var compressEffect = _actorEffects[actorId]
            .FirstOrDefault(e => e.Type == StatusEffect.Compressed && !e.IsExpired);

        return compressEffect?.Magnitude ?? 0;
    }
}
