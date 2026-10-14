namespace Acme.Workforce.Assets.Warranty.Cycle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled warranty cycle is in its life cycle.</summary>
public enum ScheduledWarrantyCycleStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One scheduled warranty cycle, with what the planners need to act on it.</summary>
public sealed class ScheduledWarrantyCycle(string code, string name, string checksum, double margin, double hours, double weight, double budget, Dictionary<string, string> attributes, ScheduledWarrantyCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Checksum { get; set; } = checksum;
    public double Margin { get; set; } = margin;
    public double Hours { get; set; } = hours;
    public double Weight { get; set; } = weight;
    public double Budget { get; set; } = budget;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public ScheduledWarrantyCycleStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled warranty cycle in status open.</summary>
    public static ScheduledWarrantyCycle NewScheduledWarrantyCycle(string code, string name)
    {
        return new ScheduledWarrantyCycle(
            code,
            name,
            "",
            0.0,
            0.0,
            0.0,
            0.0,
            new Dictionary<string, string>(),
            ScheduledWarrantyCycleStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledWarrantyCycleStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledWarrantyCycleStatus next)
    {
        switch (Status)
        {
            case ScheduledWarrantyCycleStatus.Open:
                return next == ScheduledWarrantyCycleStatus.Resolved || next == ScheduledWarrantyCycleStatus.Reopened;
            case ScheduledWarrantyCycleStatus.InReview:
                return next == ScheduledWarrantyCycleStatus.Resolved || next == ScheduledWarrantyCycleStatus.Reopened;
            case ScheduledWarrantyCycleStatus.Resolved:
                switch (next)
                {
                    case ScheduledWarrantyCycleStatus.Archived:
                        return true;
                }
                return false;
            case ScheduledWarrantyCycleStatus.Reopened:
                return next == ScheduledWarrantyCycleStatus.Archived;
        }
        return false;
    }

    public void MoveTo(ScheduledWarrantyCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled warranty cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
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

    public double ProjectedExposure()
    {
        return Math.Round(Weight + Budget, 2);
    }

    public ScheduledWarrantyCycle Copy()
    {
        return new ScheduledWarrantyCycle(Code, Name, Checksum, Margin, Hours, Weight, Budget, Attributes, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
