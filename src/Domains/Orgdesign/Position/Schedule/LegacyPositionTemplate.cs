namespace Acme.Workforce.Orgdesign.Position.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy position template is in its life cycle.</summary>
public enum LegacyPositionTemplateStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A legacy position template as the position team keeps it.</summary>
public sealed class LegacyPositionTemplate(string code, string name, int tenureMonths, string timezone, Dictionary<string, string> attributes, int durationMinutes, int attempts, string grade, bool pinned, double threshold, double rate, LegacyPositionTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Timezone { get; set; } = timezone;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public int DurationMinutes { get; set; } = durationMinutes;
    public int Attempts { get; set; } = attempts;
    public string Grade { get; set; } = grade;
    public bool Pinned { get; set; } = pinned;
    public double Threshold { get; set; } = threshold;
    public double Rate { get; set; } = rate;
    public LegacyPositionTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a legacy position template in status new.</summary>
    public static LegacyPositionTemplate NewLegacyPositionTemplate(string code, string name)
    {
        return new LegacyPositionTemplate(
            code,
            name,
            0,
            "",
            new Dictionary<string, string>(),
            0,
            0,
            "",
            false,
            0.0,
            0.0,
            LegacyPositionTemplateStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyPositionTemplateStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyPositionTemplateStatus next)
    {
        switch (Status)
        {
            case LegacyPositionTemplateStatus.New:
                switch (next)
                {
                    case LegacyPositionTemplateStatus.Running:
                        return true;
                }
                return false;
            case LegacyPositionTemplateStatus.Queued:
                switch (next)
                {
                    case LegacyPositionTemplateStatus.Running:
                        return true;
                }
                return false;
            case LegacyPositionTemplateStatus.Running:
                return next == LegacyPositionTemplateStatus.Done;
            case LegacyPositionTemplateStatus.Done:
                return next == LegacyPositionTemplateStatus.Failed;
        }
        return false;
    }

    public void MoveTo(LegacyPositionTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy position template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"legacy position template"} {Code}";
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

    public double AdjustedTotal()
    {
        return Math.Round((double)DurationMinutes - Rate, 2);
    }

    public LegacyPositionTemplate Copy()
    {
        return new LegacyPositionTemplate(
            Code,
            Name,
            TenureMonths,
            Timezone,
            Attributes,
            DurationMinutes,
            Attempts,
            Grade,
            Pinned,
            Threshold,
            Rate,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
