namespace Acme.Workforce.Orgdesign.Reorg.Plan;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict reorg budget is in its life cycle.</summary>
public enum StrictReorgBudgetStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A strict reorg budget. Its code is unique within a tenant.</summary>
public sealed class StrictReorgBudget(string code, string name, string checksum, int limit, int endsAt, string channel, string locale, string salaryBand, StrictReorgBudgetStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Checksum { get; set; } = checksum;
    public int Limit { get; set; } = limit;
    public int EndsAt { get; set; } = endsAt;
    public string Channel { get; set; } = channel;
    public string Locale { get; set; } = locale;
    public string SalaryBand { get; set; } = salaryBand;
    public StrictReorgBudgetStatus Status { get; set; } = status;

    /// <summary>Starts a strict reorg budget in status created.</summary>
    public static StrictReorgBudget NewStrictReorgBudget(string code, string name)
    {
        return new StrictReorgBudget(code, name, "", 0, 0, "", "", "", StrictReorgBudgetStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictReorgBudgetStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictReorgBudgetStatus next)
    {
        switch (Status)
        {
            case StrictReorgBudgetStatus.Created:
                return next == StrictReorgBudgetStatus.Published;
            case StrictReorgBudgetStatus.Validated:
                switch (next)
                {
                    case StrictReorgBudgetStatus.Published:
                    case StrictReorgBudgetStatus.Retired:
                        return true;
                }
                return false;
            case StrictReorgBudgetStatus.Published:
                switch (next)
                {
                    case StrictReorgBudgetStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(StrictReorgBudgetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict reorg budget {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"strict reorg budget"} {Code}";
    }

    public double NetLoad()
    {
        return Math.Round((double)EndsAt * (double)Limit, 2);
    }

    public StrictReorgBudget Copy()
    {
        return new StrictReorgBudget(Code, Name, Checksum, Limit, EndsAt, Channel, Locale, SalaryBand, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
