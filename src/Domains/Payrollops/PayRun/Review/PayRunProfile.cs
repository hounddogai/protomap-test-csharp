namespace Acme.Workforce.Payrollops.PayRun.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pay run profile is in its life cycle.</summary>
public enum PayRunProfileStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One pay run profile, with what the planners need to act on it.</summary>
public sealed class PayRunProfile(string code, string title, double discount, string salaryBand, bool archived, List<string> tags, string skill, PayRunProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Discount { get; set; } = discount;
    public string SalaryBand { get; set; } = salaryBand;
    public bool Archived { get; set; } = archived;
    public List<string> Tags { get; set; } = tags;
    public string Skill { get; set; } = skill;
    public PayRunProfileStatus Status { get; set; } = status;

    /// <summary>Starts a pay run profile in status proposed.</summary>
    public static PayRunProfile NewPayRunProfile(string code, string title)
    {
        return new PayRunProfile(code, title, 0.0, "", false, new List<string>(), "", PayRunProfileStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayRunProfileStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayRunProfileStatus next)
    {
        switch (Status)
        {
            case PayRunProfileStatus.Proposed:
                switch (next)
                {
                    case PayRunProfileStatus.Rejected:
                    case PayRunProfileStatus.Withdrawn:
                        return true;
                }
                return false;
            case PayRunProfileStatus.Approved:
                switch (next)
                {
                    case PayRunProfileStatus.Rejected:
                    case PayRunProfileStatus.Withdrawn:
                        return true;
                }
                return false;
            case PayRunProfileStatus.Rejected:
                switch (next)
                {
                    case PayRunProfileStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PayRunProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pay run profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
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

    public double MeasuredValue()
    {
        return Discount;
    }

    public PayRunProfile Copy()
    {
        return new PayRunProfile(Code, Title, Discount, SalaryBand, Archived, Tags, Skill, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
