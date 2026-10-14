namespace Acme.Workforce.Absence.Policy.PolicyRollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary policy rollup is in its life cycle.</summary>
public enum PrimaryPolicyRollupStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a primary policy rollup and its current status.</summary>
public sealed class PrimaryPolicyRollup(string code, string label, string skill, int limit, double budget, PrimaryPolicyRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Skill { get; set; } = skill;
    public int Limit { get; set; } = limit;
    public double Budget { get; set; } = budget;
    public PrimaryPolicyRollupStatus Status { get; set; } = status;

    /// <summary>Starts a primary policy rollup in status draft.</summary>
    public static PrimaryPolicyRollup NewPrimaryPolicyRollup(string code, string label)
    {
        return new PrimaryPolicyRollup(code, label, "", 0, 0.0, PrimaryPolicyRollupStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryPolicyRollupStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryPolicyRollupStatus next)
    {
        switch (Status)
        {
            case PrimaryPolicyRollupStatus.Draft:
                return next == PrimaryPolicyRollupStatus.Active || next == PrimaryPolicyRollupStatus.Suspended;
            case PrimaryPolicyRollupStatus.Pending:
                return next == PrimaryPolicyRollupStatus.Suspended || next == PrimaryPolicyRollupStatus.Closed;
            case PrimaryPolicyRollupStatus.Active:
                switch (next)
                {
                    case PrimaryPolicyRollupStatus.Closed:
                        return true;
                }
                return false;
            case PrimaryPolicyRollupStatus.Suspended:
                switch (next)
                {
                    case PrimaryPolicyRollupStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PrimaryPolicyRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary policy rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double NetTotal()
    {
        return Math.Round(Budget * (double)Limit, 2);
    }

    public PrimaryPolicyRollup Copy()
    {
        return new PrimaryPolicyRollup(Code, Label, Skill, Limit, Budget, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
