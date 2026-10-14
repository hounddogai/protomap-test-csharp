namespace Acme.Workforce.Workplace.Desk.Variance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global desk variance is in its life cycle.</summary>
public enum GlobalDeskVarianceStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a global desk variance and its current status.</summary>
public sealed class GlobalDeskVariance(string code, string label, string employeeId, int quantity, double cost, string salaryBand, int batchSize, GlobalDeskVarianceStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string EmployeeId { get; set; } = employeeId;
    public int Quantity { get; set; } = quantity;
    public double Cost { get; set; } = cost;
    public string SalaryBand { get; set; } = salaryBand;
    public int BatchSize { get; set; } = batchSize;
    public GlobalDeskVarianceStatus Status { get; set; } = status;

    /// <summary>Starts a global desk variance in status draft.</summary>
    public static GlobalDeskVariance NewGlobalDeskVariance(string code, string label)
    {
        return new GlobalDeskVariance(code, label, "", 0, 0.0, "", 0, GlobalDeskVarianceStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalDeskVarianceStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalDeskVarianceStatus next)
    {
        switch (Status)
        {
            case GlobalDeskVarianceStatus.Draft:
                switch (next)
                {
                    case GlobalDeskVarianceStatus.Closed:
                        return true;
                }
                return false;
            case GlobalDeskVarianceStatus.Pending:
                switch (next)
                {
                    case GlobalDeskVarianceStatus.Suspended:
                    case GlobalDeskVarianceStatus.Closed:
                        return true;
                }
                return false;
            case GlobalDeskVarianceStatus.Active:
                switch (next)
                {
                    case GlobalDeskVarianceStatus.Suspended:
                    case GlobalDeskVarianceStatus.Closed:
                        return true;
                }
                return false;
            case GlobalDeskVarianceStatus.Suspended:
                switch (next)
                {
                    case GlobalDeskVarianceStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(GlobalDeskVarianceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global desk variance {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double NetVolume()
    {
        return Math.Round(Cost + (double)BatchSize, 2);
    }

    public GlobalDeskVariance Copy()
    {
        return new GlobalDeskVariance(Code, Label, EmployeeId, Quantity, Cost, SalaryBand, BatchSize, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
