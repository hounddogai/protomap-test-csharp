namespace Acme.Workforce.Talent.Nomination.Rule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a nomination rule is in its life cycle.</summary>
public enum NominationRuleStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A nomination rule. Its code is unique within a tenant.</summary>
public sealed class NominationRule(string code, string label, string checksum, string summary, string title, string channel, double score, NominationRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Checksum { get; set; } = checksum;
    public string Summary { get; set; } = summary;
    public string Title { get; set; } = title;
    public string Channel { get; set; } = channel;
    public double Score { get; set; } = score;
    public NominationRuleStatus Status { get; set; } = status;

    /// <summary>Starts a nomination rule in status new.</summary>
    public static NominationRule NewNominationRule(string code, string label)
    {
        return new NominationRule(code, label, "", "", "", "", 0.0, NominationRuleStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case NominationRuleStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(NominationRuleStatus next)
    {
        switch (Status)
        {
            case NominationRuleStatus.New:
                switch (next)
                {
                    case NominationRuleStatus.Queued:
                    case NominationRuleStatus.Done:
                        return true;
                }
                return false;
            case NominationRuleStatus.Queued:
                switch (next)
                {
                    case NominationRuleStatus.Running:
                    case NominationRuleStatus.Done:
                        return true;
                }
                return false;
            case NominationRuleStatus.Running:
                return next == NominationRuleStatus.Done || next == NominationRuleStatus.Failed;
            case NominationRuleStatus.Done:
                switch (next)
                {
                    case NominationRuleStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(NominationRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"nomination rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double MeasuredValue()
    {
        return Score;
    }

    public NominationRule Copy()
    {
        return new NominationRule(Code, Label, Checksum, Summary, Title, Channel, Score, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
