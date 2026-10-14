namespace Acme.Workforce.Engagement.PulseSurvey.Manifest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pulse survey draft is in its life cycle.</summary>
public enum PulseSurveyDraftStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A pulse survey draft as the pulse survey team keeps it.</summary>
public sealed class PulseSurveyDraft(string code, string name, bool active, string managerId, string salaryBand, string location, double amount, PulseSurveyDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Active { get; set; } = active;
    public string ManagerId { get; set; } = managerId;
    public string SalaryBand { get; set; } = salaryBand;
    public string Location { get; set; } = location;
    public double Amount { get; set; } = amount;
    public PulseSurveyDraftStatus Status { get; set; } = status;

    /// <summary>Starts a pulse survey draft in status draft.</summary>
    public static PulseSurveyDraft NewPulseSurveyDraft(string code, string name)
    {
        return new PulseSurveyDraft(code, name, false, "", "", "", 0.0, PulseSurveyDraftStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PulseSurveyDraftStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PulseSurveyDraftStatus next)
    {
        switch (Status)
        {
            case PulseSurveyDraftStatus.Draft:
                switch (next)
                {
                    case PulseSurveyDraftStatus.Closed:
                        return true;
                }
                return false;
            case PulseSurveyDraftStatus.Pending:
                switch (next)
                {
                    case PulseSurveyDraftStatus.Active:
                    case PulseSurveyDraftStatus.Suspended:
                        return true;
                }
                return false;
            case PulseSurveyDraftStatus.Active:
                return next == PulseSurveyDraftStatus.Suspended;
            case PulseSurveyDraftStatus.Suspended:
                switch (next)
                {
                    case PulseSurveyDraftStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PulseSurveyDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pulse survey draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"pulse survey draft"} {Code}";
    }

    public double MeasuredValue()
    {
        return Amount;
    }

    public PulseSurveyDraft Copy()
    {
        return new PulseSurveyDraft(Code, Name, Active, ManagerId, SalaryBand, Location, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
