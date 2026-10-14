namespace Acme.Workforce.Orgdesign.CostModel.ModelEntry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a cost model record is in its life cycle.</summary>
public enum CostModelRecordStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A cost model record. Its code is unique within a tenant.</summary>
public sealed class CostModelRecord(string code, string title, int revision, int sequence, int limit, string location, string checksum, string segment, CostModelRecordStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Revision { get; set; } = revision;
    public int Sequence { get; set; } = sequence;
    public int Limit { get; set; } = limit;
    public string Location { get; set; } = location;
    public string Checksum { get; set; } = checksum;
    public string Segment { get; set; } = segment;
    public CostModelRecordStatus Status { get; set; } = status;

    /// <summary>Starts a cost model record in status proposed.</summary>
    public static CostModelRecord NewCostModelRecord(string code, string title)
    {
        return new CostModelRecord(code, title, 0, 0, 0, "", "", "", CostModelRecordStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CostModelRecordStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CostModelRecordStatus next)
    {
        switch (Status)
        {
            case CostModelRecordStatus.Proposed:
                switch (next)
                {
                    case CostModelRecordStatus.Rejected:
                        return true;
                }
                return false;
            case CostModelRecordStatus.Approved:
                switch (next)
                {
                    case CostModelRecordStatus.Withdrawn:
                        return true;
                }
                return false;
            case CostModelRecordStatus.Rejected:
                return next == CostModelRecordStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(CostModelRecordStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"cost model record {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double WeightedLoad()
    {
        return Math.Round((double)Limit * (double)Sequence, 2);
    }

    public CostModelRecord Copy()
    {
        return new CostModelRecord(Code, Title, Revision, Sequence, Limit, Location, Checksum, Segment, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
