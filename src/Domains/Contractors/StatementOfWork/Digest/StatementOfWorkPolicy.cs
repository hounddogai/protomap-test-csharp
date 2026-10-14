namespace Acme.Workforce.Contractors.StatementOfWork.Digest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a statement of work policy is in its life cycle.</summary>
public enum StatementOfWorkPolicyStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One statement of work policy, with what the planners need to act on it.</summary>
public sealed class StatementOfWorkPolicy(string code, string label, double rate, int startsAt, string locale, bool archived, string country, string summary, string contactEmail, string grade, string managerId, StatementOfWorkPolicyStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Rate { get; set; } = rate;
    public int StartsAt { get; set; } = startsAt;
    public string Locale { get; set; } = locale;
    public bool Archived { get; set; } = archived;
    public string Country { get; set; } = country;
    public string Summary { get; set; } = summary;
    public string ContactEmail { get; set; } = contactEmail;
    public string Grade { get; set; } = grade;
    public string ManagerId { get; set; } = managerId;
    public StatementOfWorkPolicyStatus Status { get; set; } = status;

    /// <summary>Starts a statement of work policy in status open.</summary>
    public static StatementOfWorkPolicy NewStatementOfWorkPolicy(string code, string label)
    {
        return new StatementOfWorkPolicy(
            code,
            label,
            0.0,
            0,
            "",
            false,
            "",
            "",
            "",
            "",
            "",
            StatementOfWorkPolicyStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StatementOfWorkPolicyStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StatementOfWorkPolicyStatus next)
    {
        switch (Status)
        {
            case StatementOfWorkPolicyStatus.Open:
                switch (next)
                {
                    case StatementOfWorkPolicyStatus.Archived:
                        return true;
                }
                return false;
            case StatementOfWorkPolicyStatus.InReview:
                return next == StatementOfWorkPolicyStatus.Archived;
            case StatementOfWorkPolicyStatus.Resolved:
                switch (next)
                {
                    case StatementOfWorkPolicyStatus.Archived:
                        return true;
                }
                return false;
            case StatementOfWorkPolicyStatus.Reopened:
                switch (next)
                {
                    case StatementOfWorkPolicyStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(StatementOfWorkPolicyStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"statement of work policy {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double AdjustedLoad()
    {
        return Math.Round(Rate + (double)StartsAt, 2);
    }

    public StatementOfWorkPolicy Copy()
    {
        return new StatementOfWorkPolicy(
            Code,
            Label,
            Rate,
            StartsAt,
            Locale,
            Archived,
            Country,
            Summary,
            ContactEmail,
            Grade,
            ManagerId,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
