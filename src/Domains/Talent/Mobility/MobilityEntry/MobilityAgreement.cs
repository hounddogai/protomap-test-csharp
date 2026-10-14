namespace Acme.Workforce.Talent.Mobility.MobilityEntry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a mobility agreement is in its life cycle.</summary>
public enum MobilityAgreementStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a mobility agreement and its current status.</summary>
public sealed class MobilityAgreement(string code, string label, int graceDays, double rating, string segment, double maximum, string skill, string source, int sequence, int quantity, string reviewer, MobilityAgreementStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int GraceDays { get; set; } = graceDays;
    public double Rating { get; set; } = rating;
    public string Segment { get; set; } = segment;
    public double Maximum { get; set; } = maximum;
    public string Skill { get; set; } = skill;
    public string Source { get; set; } = source;
    public int Sequence { get; set; } = sequence;
    public int Quantity { get; set; } = quantity;
    public string Reviewer { get; set; } = reviewer;
    public MobilityAgreementStatus Status { get; set; } = status;

    /// <summary>Starts a mobility agreement in status new.</summary>
    public static MobilityAgreement NewMobilityAgreement(string code, string label)
    {
        return new MobilityAgreement(code, label, 0, 0.0, "", 0.0, "", "", 0, 0, "", MobilityAgreementStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case MobilityAgreementStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(MobilityAgreementStatus next)
    {
        switch (Status)
        {
            case MobilityAgreementStatus.New:
                return next == MobilityAgreementStatus.Queued || next == MobilityAgreementStatus.Failed;
            case MobilityAgreementStatus.Queued:
                switch (next)
                {
                    case MobilityAgreementStatus.Failed:
                        return true;
                }
                return false;
            case MobilityAgreementStatus.Running:
                return next == MobilityAgreementStatus.Done || next == MobilityAgreementStatus.Failed;
            case MobilityAgreementStatus.Done:
                switch (next)
                {
                    case MobilityAgreementStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(MobilityAgreementStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"mobility agreement {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double WeightedVolume()
    {
        return Math.Round((double)Sequence * Maximum, 2);
    }

    public MobilityAgreement Copy()
    {
        return new MobilityAgreement(
            Code,
            Label,
            GraceDays,
            Rating,
            Segment,
            Maximum,
            Skill,
            Source,
            Sequence,
            Quantity,
            Reviewer,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
