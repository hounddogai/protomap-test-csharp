namespace Acme.Workforce.Payrollops.PayRun.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pay run assignment is in its life cycle.</summary>
public enum PayRunAssignmentStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A pay run assignment as the pay run team keeps it.</summary>
public sealed class PayRunAssignment(string code, string label, double rating, int attempts, string summary, string location, string reviewer, PayRunAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Rating { get; set; } = rating;
    public int Attempts { get; set; } = attempts;
    public string Summary { get; set; } = summary;
    public string Location { get; set; } = location;
    public string Reviewer { get; set; } = reviewer;
    public PayRunAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a pay run assignment in status draft.</summary>
    public static PayRunAssignment NewPayRunAssignment(string code, string label)
    {
        return new PayRunAssignment(code, label, 0.0, 0, "", "", "", PayRunAssignmentStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayRunAssignmentStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayRunAssignmentStatus next)
    {
        switch (Status)
        {
            case PayRunAssignmentStatus.Draft:
                return next == PayRunAssignmentStatus.Active || next == PayRunAssignmentStatus.Suspended;
            case PayRunAssignmentStatus.Pending:
                switch (next)
                {
                    case PayRunAssignmentStatus.Suspended:
                        return true;
                }
                return false;
            case PayRunAssignmentStatus.Active:
                return next == PayRunAssignmentStatus.Closed;
            case PayRunAssignmentStatus.Suspended:
                switch (next)
                {
                    case PayRunAssignmentStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PayRunAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pay run assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double EffectiveLoad()
    {
        return Math.Round((double)Attempts + Rating, 2);
    }

    public PayRunAssignment Copy()
    {
        return new PayRunAssignment(Code, Label, Rating, Attempts, Summary, Location, Reviewer, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
