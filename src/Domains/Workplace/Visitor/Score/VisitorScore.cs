namespace Acme.Workforce.Workplace.Visitor.Score;

using Acme.Workforce.Kit.Text;

/// <summary>Where a visitor score is in its life cycle.</summary>
public enum VisitorScoreStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a visitor score and its current status.</summary>
public sealed class VisitorScore(string code, string name, int updatedAt, double rate, double ceiling, double discount, VisitorScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int UpdatedAt { get; set; } = updatedAt;
    public double Rate { get; set; } = rate;
    public double Ceiling { get; set; } = ceiling;
    public double Discount { get; set; } = discount;
    public VisitorScoreStatus Status { get; set; } = status;

    /// <summary>Starts a visitor score in status new.</summary>
    public static VisitorScore NewVisitorScore(string code, string name)
    {
        return new VisitorScore(code, name, 0, 0.0, 0.0, 0.0, VisitorScoreStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case VisitorScoreStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(VisitorScoreStatus next)
    {
        switch (Status)
        {
            case VisitorScoreStatus.New:
                return next == VisitorScoreStatus.Done || next == VisitorScoreStatus.Failed;
            case VisitorScoreStatus.Queued:
                switch (next)
                {
                    case VisitorScoreStatus.Failed:
                        return true;
                }
                return false;
            case VisitorScoreStatus.Running:
                return next == VisitorScoreStatus.Done || next == VisitorScoreStatus.Failed;
            case VisitorScoreStatus.Done:
                switch (next)
                {
                    case VisitorScoreStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(VisitorScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"visitor score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double AdjustedTotal()
    {
        return Math.Round(Rate - Ceiling, 2);
    }

    public VisitorScore Copy()
    {
        return new VisitorScore(Code, Name, UpdatedAt, Rate, Ceiling, Discount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
