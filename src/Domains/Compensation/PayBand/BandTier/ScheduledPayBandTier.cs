namespace Acme.Workforce.Compensation.PayBand.BandTier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled pay band tier is in its life cycle.</summary>
public enum ScheduledPayBandTierStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A scheduled pay band tier. Its code is unique within a tenant.</summary>
public sealed class ScheduledPayBandTier(string code, string label, int graceDays, double hours, Dictionary<string, string> attributes, double ratio, string contactEmail, ScheduledPayBandTierStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int GraceDays { get; set; } = graceDays;
    public double Hours { get; set; } = hours;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public double Ratio { get; set; } = ratio;
    public string ContactEmail { get; set; } = contactEmail;
    public ScheduledPayBandTierStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled pay band tier in status new.</summary>
    public static ScheduledPayBandTier NewScheduledPayBandTier(string code, string label)
    {
        return new ScheduledPayBandTier(
            code,
            label,
            0,
            0.0,
            new Dictionary<string, string>(),
            0.0,
            "",
            ScheduledPayBandTierStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledPayBandTierStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledPayBandTierStatus next)
    {
        switch (Status)
        {
            case ScheduledPayBandTierStatus.New:
                return next == ScheduledPayBandTierStatus.Running;
            case ScheduledPayBandTierStatus.Queued:
                switch (next)
                {
                    case ScheduledPayBandTierStatus.Failed:
                        return true;
                }
                return false;
            case ScheduledPayBandTierStatus.Running:
                switch (next)
                {
                    case ScheduledPayBandTierStatus.Done:
                    case ScheduledPayBandTierStatus.Failed:
                        return true;
                }
                return false;
            case ScheduledPayBandTierStatus.Done:
                switch (next)
                {
                    case ScheduledPayBandTierStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ScheduledPayBandTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled pay band tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"scheduled pay band tier"} {Code}";
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

    public double WeightedVolume()
    {
        return Math.Round((double)GraceDays - Hours, 2);
    }

    public ScheduledPayBandTier Copy()
    {
        return new ScheduledPayBandTier(Code, Label, GraceDays, Hours, Attributes, Ratio, ContactEmail, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
