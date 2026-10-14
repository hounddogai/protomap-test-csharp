namespace Acme.Workforce.Orgdesign.Headcount.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a headcount record is in its life cycle.</summary>
public enum HeadcountRecordStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One headcount record, with what the planners need to act on it.</summary>
public sealed class HeadcountRecord(string code, string name, string label, int tenureMonths, bool approved, HeadcountRecordStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Label { get; set; } = label;
    public int TenureMonths { get; set; } = tenureMonths;
    public bool Approved { get; set; } = approved;
    public HeadcountRecordStatus Status { get; set; } = status;

    /// <summary>Starts a headcount record in status created.</summary>
    public static HeadcountRecord NewHeadcountRecord(string code, string name)
    {
        return new HeadcountRecord(code, name, "", 0, false, HeadcountRecordStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case HeadcountRecordStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(HeadcountRecordStatus next)
    {
        switch (Status)
        {
            case HeadcountRecordStatus.Created:
                switch (next)
                {
                    case HeadcountRecordStatus.Retired:
                        return true;
                }
                return false;
            case HeadcountRecordStatus.Validated:
                switch (next)
                {
                    case HeadcountRecordStatus.Published:
                        return true;
                }
                return false;
            case HeadcountRecordStatus.Published:
                switch (next)
                {
                    case HeadcountRecordStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(HeadcountRecordStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"headcount record {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Name}]";
    }

    public double MeasuredValue()
    {
        return (double)TenureMonths;
    }

    public HeadcountRecord Copy()
    {
        return new HeadcountRecord(Code, Name, Label, TenureMonths, Approved, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
