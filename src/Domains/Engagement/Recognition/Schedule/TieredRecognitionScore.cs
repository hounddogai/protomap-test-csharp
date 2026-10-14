namespace Acme.Workforce.Engagement.Recognition.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered recognition score is in its life cycle.</summary>
public enum TieredRecognitionScoreStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A tiered recognition score as the recognition team keeps it.</summary>
public sealed class TieredRecognitionScore(string code, string name, double weight, string phone, double hours, TieredRecognitionScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Weight { get; set; } = weight;
    public string Phone { get; set; } = phone;
    public double Hours { get; set; } = hours;
    public TieredRecognitionScoreStatus Status { get; set; } = status;

    /// <summary>Starts a tiered recognition score in status draft.</summary>
    public static TieredRecognitionScore NewTieredRecognitionScore(string code, string name)
    {
        return new TieredRecognitionScore(code, name, 0.0, "", 0.0, TieredRecognitionScoreStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredRecognitionScoreStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredRecognitionScoreStatus next)
    {
        switch (Status)
        {
            case TieredRecognitionScoreStatus.Draft:
                switch (next)
                {
                    case TieredRecognitionScoreStatus.Active:
                        return true;
                }
                return false;
            case TieredRecognitionScoreStatus.Pending:
                switch (next)
                {
                    case TieredRecognitionScoreStatus.Active:
                        return true;
                }
                return false;
            case TieredRecognitionScoreStatus.Active:
                return next == TieredRecognitionScoreStatus.Suspended || next == TieredRecognitionScoreStatus.Closed;
            case TieredRecognitionScoreStatus.Suspended:
                switch (next)
                {
                    case TieredRecognitionScoreStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TieredRecognitionScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered recognition score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double NetVolume()
    {
        return Math.Round(Weight - Hours, 2);
    }

    public TieredRecognitionScore Copy()
    {
        return new TieredRecognitionScore(Code, Name, Weight, Phone, Hours, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
