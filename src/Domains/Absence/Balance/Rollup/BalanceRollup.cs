namespace Acme.Workforce.Absence.Balance.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a balance rollup is in its life cycle.</summary>
public enum BalanceRollupStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A balance rollup as the balance team keeps it.</summary>
public sealed class BalanceRollup(string code, string name, string title, string badge, bool archived, string phone, string salaryBand, double margin, int updatedAt, BalanceRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Title { get; set; } = title;
    public string Badge { get; set; } = badge;
    public bool Archived { get; set; } = archived;
    public string Phone { get; set; } = phone;
    public string SalaryBand { get; set; } = salaryBand;
    public double Margin { get; set; } = margin;
    public int UpdatedAt { get; set; } = updatedAt;
    public BalanceRollupStatus Status { get; set; } = status;

    /// <summary>Starts a balance rollup in status new.</summary>
    public static BalanceRollup NewBalanceRollup(string code, string name)
    {
        return new BalanceRollup(code, name, "", "", false, "", "", 0.0, 0, BalanceRollupStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BalanceRollupStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BalanceRollupStatus next)
    {
        switch (Status)
        {
            case BalanceRollupStatus.New:
                switch (next)
                {
                    case BalanceRollupStatus.Failed:
                        return true;
                }
                return false;
            case BalanceRollupStatus.Queued:
                switch (next)
                {
                    case BalanceRollupStatus.Running:
                    case BalanceRollupStatus.Failed:
                        return true;
                }
                return false;
            case BalanceRollupStatus.Running:
                return next == BalanceRollupStatus.Failed;
            case BalanceRollupStatus.Done:
                switch (next)
                {
                    case BalanceRollupStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BalanceRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"balance rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double ProjectedLoad()
    {
        return Math.Round((double)UpdatedAt * Margin, 2);
    }

    public BalanceRollup Copy()
    {
        return new BalanceRollup(Code, Name, Title, Badge, Archived, Phone, SalaryBand, Margin, UpdatedAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
