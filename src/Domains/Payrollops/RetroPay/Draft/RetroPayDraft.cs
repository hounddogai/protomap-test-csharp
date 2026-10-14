namespace Acme.Workforce.Payrollops.RetroPay.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a retro pay draft is in its life cycle.</summary>
public enum RetroPayDraftStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A retro pay draft. Its code is unique within a tenant.</summary>
public sealed class RetroPayDraft(string code, string title, string phone, string name, double hours, string salaryBand, double weight, int priority, int retries, RetroPayDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Phone { get; set; } = phone;
    public string Name { get; set; } = name;
    public double Hours { get; set; } = hours;
    public string SalaryBand { get; set; } = salaryBand;
    public double Weight { get; set; } = weight;
    public int Priority { get; set; } = priority;
    public int Retries { get; set; } = retries;
    public RetroPayDraftStatus Status { get; set; } = status;

    /// <summary>Starts a retro pay draft in status created.</summary>
    public static RetroPayDraft NewRetroPayDraft(string code, string title)
    {
        return new RetroPayDraft(code, title, "", "", 0.0, "", 0.0, 0, 0, RetroPayDraftStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RetroPayDraftStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RetroPayDraftStatus next)
    {
        switch (Status)
        {
            case RetroPayDraftStatus.Created:
                return next == RetroPayDraftStatus.Validated || next == RetroPayDraftStatus.Published;
            case RetroPayDraftStatus.Validated:
                switch (next)
                {
                    case RetroPayDraftStatus.Published:
                        return true;
                }
                return false;
            case RetroPayDraftStatus.Published:
                return next == RetroPayDraftStatus.Retired;
        }
        return false;
    }

    public void MoveTo(RetroPayDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"retro pay draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"retro pay draft"} {Code}";
    }

    public double ProjectedValue()
    {
        return Math.Round(Weight + (double)Retries, 2);
    }

    public RetroPayDraft Copy()
    {
        return new RetroPayDraft(Code, Title, Phone, Name, Hours, SalaryBand, Weight, Priority, Retries, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
