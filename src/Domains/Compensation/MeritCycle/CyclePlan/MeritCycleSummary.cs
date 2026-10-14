namespace Acme.Workforce.Compensation.MeritCycle.CyclePlan;

using Acme.Workforce.Kit.Text;

/// <summary>Where a merit cycle summary is in its life cycle.</summary>
public enum MeritCycleSummaryStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a merit cycle summary and its current status.</summary>
public sealed class MeritCycleSummary(string code, string label, string badge, string notes, double margin, double weight, Dictionary<string, string> attributes, string location, MeritCycleSummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Badge { get; set; } = badge;
    public string Notes { get; set; } = notes;
    public double Margin { get; set; } = margin;
    public double Weight { get; set; } = weight;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Location { get; set; } = location;
    public MeritCycleSummaryStatus Status { get; set; } = status;

    /// <summary>Starts a merit cycle summary in status proposed.</summary>
    public static MeritCycleSummary NewMeritCycleSummary(string code, string label)
    {
        return new MeritCycleSummary(
            code,
            label,
            "",
            "",
            0.0,
            0.0,
            new Dictionary<string, string>(),
            "",
            MeritCycleSummaryStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case MeritCycleSummaryStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(MeritCycleSummaryStatus next)
    {
        switch (Status)
        {
            case MeritCycleSummaryStatus.Proposed:
                return next == MeritCycleSummaryStatus.Approved || next == MeritCycleSummaryStatus.Withdrawn;
            case MeritCycleSummaryStatus.Approved:
                switch (next)
                {
                    case MeritCycleSummaryStatus.Rejected:
                    case MeritCycleSummaryStatus.Withdrawn:
                        return true;
                }
                return false;
            case MeritCycleSummaryStatus.Rejected:
                return next == MeritCycleSummaryStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(MeritCycleSummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"merit cycle summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
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

    public double ProjectedLoad()
    {
        return Math.Round(Weight + Margin, 2);
    }

    public MeritCycleSummary Copy()
    {
        return new MeritCycleSummary(Code, Label, Badge, Notes, Margin, Weight, Attributes, Location, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
