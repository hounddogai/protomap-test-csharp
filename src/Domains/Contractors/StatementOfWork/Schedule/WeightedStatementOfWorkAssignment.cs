namespace Acme.Workforce.Contractors.StatementOfWork.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted statement of work assignment is in its life cycle.</summary>
public enum WeightedStatementOfWorkAssignmentStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A weighted statement of work assignment as the statement of work team keeps it.</summary>
public sealed class WeightedStatementOfWorkAssignment(string code, string label, string contactEmail, int tenureMonths, double weight, List<string> tags, bool active, string locale, double ceiling, WeightedStatementOfWorkAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string ContactEmail { get; set; } = contactEmail;
    public int TenureMonths { get; set; } = tenureMonths;
    public double Weight { get; set; } = weight;
    public List<string> Tags { get; set; } = tags;
    public bool Active { get; set; } = active;
    public string Locale { get; set; } = locale;
    public double Ceiling { get; set; } = ceiling;
    public WeightedStatementOfWorkAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a weighted statement of work assignment in status requested.</summary>
    public static WeightedStatementOfWorkAssignment NewWeightedStatementOfWorkAssignment(string code, string label)
    {
        return new WeightedStatementOfWorkAssignment(
            code,
            label,
            "",
            0,
            0.0,
            new List<string>(),
            false,
            "",
            0.0,
            WeightedStatementOfWorkAssignmentStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedStatementOfWorkAssignmentStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedStatementOfWorkAssignmentStatus next)
    {
        switch (Status)
        {
            case WeightedStatementOfWorkAssignmentStatus.Requested:
                switch (next)
                {
                    case WeightedStatementOfWorkAssignmentStatus.Expired:
                        return true;
                }
                return false;
            case WeightedStatementOfWorkAssignmentStatus.Granted:
                return next == WeightedStatementOfWorkAssignmentStatus.Expired;
            case WeightedStatementOfWorkAssignmentStatus.Revoked:
                switch (next)
                {
                    case WeightedStatementOfWorkAssignmentStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(WeightedStatementOfWorkAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted statement of work assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double WeightedTotal()
    {
        return Math.Round(Ceiling * (double)TenureMonths, 2);
    }

    public WeightedStatementOfWorkAssignment Copy()
    {
        return new WeightedStatementOfWorkAssignment(
            Code,
            Label,
            ContactEmail,
            TenureMonths,
            Weight,
            Tags,
            Active,
            Locale,
            Ceiling,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
