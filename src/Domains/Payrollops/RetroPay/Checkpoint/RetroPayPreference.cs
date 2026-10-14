namespace Acme.Workforce.Payrollops.RetroPay.Checkpoint;

using Acme.Workforce.Kit.Text;

/// <summary>Where a retro pay preference is in its life cycle.</summary>
public enum RetroPayPreferenceStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a retro pay preference and its current status.</summary>
public sealed class RetroPayPreference(string code, string name, double fte, double maximum, string label, bool archived, string costCenter, double score, int batchSize, Dictionary<string, string> attributes, RetroPayPreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Fte { get; set; } = fte;
    public double Maximum { get; set; } = maximum;
    public string Label { get; set; } = label;
    public bool Archived { get; set; } = archived;
    public string CostCenter { get; set; } = costCenter;
    public double Score { get; set; } = score;
    public int BatchSize { get; set; } = batchSize;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public RetroPayPreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a retro pay preference in status new.</summary>
    public static RetroPayPreference NewRetroPayPreference(string code, string name)
    {
        return new RetroPayPreference(
            code,
            name,
            0.0,
            0.0,
            "",
            false,
            "",
            0.0,
            0,
            new Dictionary<string, string>(),
            RetroPayPreferenceStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RetroPayPreferenceStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RetroPayPreferenceStatus next)
    {
        switch (Status)
        {
            case RetroPayPreferenceStatus.New:
                return next == RetroPayPreferenceStatus.Queued;
            case RetroPayPreferenceStatus.Queued:
                switch (next)
                {
                    case RetroPayPreferenceStatus.Done:
                    case RetroPayPreferenceStatus.Failed:
                        return true;
                }
                return false;
            case RetroPayPreferenceStatus.Running:
                return next == RetroPayPreferenceStatus.Done || next == RetroPayPreferenceStatus.Failed;
            case RetroPayPreferenceStatus.Done:
                return next == RetroPayPreferenceStatus.Failed;
        }
        return false;
    }

    public void MoveTo(RetroPayPreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"retro pay preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Name}";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double NetTotal()
    {
        return Math.Round((double)BatchSize - Fte, 2);
    }

    public RetroPayPreference Copy()
    {
        return new RetroPayPreference(
            Code,
            Name,
            Fte,
            Maximum,
            Label,
            Archived,
            CostCenter,
            Score,
            BatchSize,
            Attributes,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
