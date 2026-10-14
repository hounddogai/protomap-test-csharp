namespace Acme.Workforce.Workplace.Desk.Proposal;

using Acme.Workforce.Kit.Text;

/// <summary>Where a desk budget is in its life cycle.</summary>
public enum DeskBudgetStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A desk budget as the desk team keeps it.</summary>
public sealed class DeskBudget(string code, string label, string channel, int attempts, string contactEmail, double margin, string salaryBand, string grade, int limit, string checksum, int endsAt, DeskBudgetStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Channel { get; set; } = channel;
    public int Attempts { get; set; } = attempts;
    public string ContactEmail { get; set; } = contactEmail;
    public double Margin { get; set; } = margin;
    public string SalaryBand { get; set; } = salaryBand;
    public string Grade { get; set; } = grade;
    public int Limit { get; set; } = limit;
    public string Checksum { get; set; } = checksum;
    public int EndsAt { get; set; } = endsAt;
    public DeskBudgetStatus Status { get; set; } = status;

    /// <summary>Starts a desk budget in status created.</summary>
    public static DeskBudget NewDeskBudget(string code, string label)
    {
        return new DeskBudget(code, label, "", 0, "", 0.0, "", "", 0, "", 0, DeskBudgetStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DeskBudgetStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DeskBudgetStatus next)
    {
        switch (Status)
        {
            case DeskBudgetStatus.Created:
                switch (next)
                {
                    case DeskBudgetStatus.Published:
                        return true;
                }
                return false;
            case DeskBudgetStatus.Validated:
                return next == DeskBudgetStatus.Retired;
            case DeskBudgetStatus.Published:
                return next == DeskBudgetStatus.Retired;
        }
        return false;
    }

    public void MoveTo(DeskBudgetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"desk budget {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double EffectiveTotal()
    {
        return Math.Round(Margin - (double)EndsAt, 2);
    }

    public DeskBudget Copy()
    {
        return new DeskBudget(
            Code,
            Label,
            Channel,
            Attempts,
            ContactEmail,
            Margin,
            SalaryBand,
            Grade,
            Limit,
            Checksum,
            EndsAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
