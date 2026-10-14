namespace Acme.Workforce.Payrollops.RetroPay.Cycle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a retro pay record is in its life cycle.</summary>
public enum RetroPayRecordStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A retro pay record as the retro pay team keeps it.</summary>
public sealed class RetroPayRecord(string code, string name, Dictionary<string, string> attributes, string salaryBand, int leaveDays, double minimum, RetroPayRecordStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string SalaryBand { get; set; } = salaryBand;
    public int LeaveDays { get; set; } = leaveDays;
    public double Minimum { get; set; } = minimum;
    public RetroPayRecordStatus Status { get; set; } = status;

    /// <summary>Starts a retro pay record in status draft.</summary>
    public static RetroPayRecord NewRetroPayRecord(string code, string name)
    {
        return new RetroPayRecord(
            code,
            name,
            new Dictionary<string, string>(),
            "",
            0,
            0.0,
            RetroPayRecordStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RetroPayRecordStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RetroPayRecordStatus next)
    {
        switch (Status)
        {
            case RetroPayRecordStatus.Draft:
                switch (next)
                {
                    case RetroPayRecordStatus.Active:
                    case RetroPayRecordStatus.Suspended:
                        return true;
                }
                return false;
            case RetroPayRecordStatus.Pending:
                return next == RetroPayRecordStatus.Active || next == RetroPayRecordStatus.Suspended;
            case RetroPayRecordStatus.Active:
                switch (next)
                {
                    case RetroPayRecordStatus.Suspended:
                    case RetroPayRecordStatus.Closed:
                        return true;
                }
                return false;
            case RetroPayRecordStatus.Suspended:
                switch (next)
                {
                    case RetroPayRecordStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RetroPayRecordStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"retro pay record {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double WeightedTotal()
    {
        return Math.Round((double)LeaveDays * Minimum, 2);
    }

    public RetroPayRecord Copy()
    {
        return new RetroPayRecord(Code, Name, Attributes, SalaryBand, LeaveDays, Minimum, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
