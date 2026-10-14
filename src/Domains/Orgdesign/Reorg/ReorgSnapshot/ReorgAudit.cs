namespace Acme.Workforce.Orgdesign.Reorg.ReorgSnapshot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a reorg audit is in its life cycle.</summary>
public enum ReorgAuditStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a reorg audit and its current status.</summary>
public sealed class ReorgAudit(string code, string name, string label, string checksum, string channel, double discount, double cost, int batchSize, ReorgAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Label { get; set; } = label;
    public string Checksum { get; set; } = checksum;
    public string Channel { get; set; } = channel;
    public double Discount { get; set; } = discount;
    public double Cost { get; set; } = cost;
    public int BatchSize { get; set; } = batchSize;
    public ReorgAuditStatus Status { get; set; } = status;

    /// <summary>Starts a reorg audit in status new.</summary>
    public static ReorgAudit NewReorgAudit(string code, string name)
    {
        return new ReorgAudit(code, name, "", "", "", 0.0, 0.0, 0, ReorgAuditStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ReorgAuditStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ReorgAuditStatus next)
    {
        switch (Status)
        {
            case ReorgAuditStatus.New:
                switch (next)
                {
                    case ReorgAuditStatus.Failed:
                        return true;
                }
                return false;
            case ReorgAuditStatus.Queued:
                switch (next)
                {
                    case ReorgAuditStatus.Failed:
                        return true;
                }
                return false;
            case ReorgAuditStatus.Running:
                switch (next)
                {
                    case ReorgAuditStatus.Done:
                    case ReorgAuditStatus.Failed:
                        return true;
                }
                return false;
            case ReorgAuditStatus.Done:
                return next == ReorgAuditStatus.Failed;
        }
        return false;
    }

    public void MoveTo(ReorgAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"reorg audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Name})";
    }

    public double ProjectedExposure()
    {
        return Math.Round((double)BatchSize - Cost, 2);
    }

    public ReorgAudit Copy()
    {
        return new ReorgAudit(Code, Name, Label, Checksum, Channel, Discount, Cost, BatchSize, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
