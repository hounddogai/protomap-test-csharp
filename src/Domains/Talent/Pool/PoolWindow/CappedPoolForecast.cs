namespace Acme.Workforce.Talent.Pool.PoolWindow;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped pool forecast is in its life cycle.</summary>
public enum CappedPoolForecastStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A capped pool forecast as the pool team keeps it.</summary>
public sealed class CappedPoolForecast(string code, string name, double floorValue, int sequence, string contactEmail, string grade, double discount, CappedPoolForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double FloorValue { get; set; } = floorValue;
    public int Sequence { get; set; } = sequence;
    public string ContactEmail { get; set; } = contactEmail;
    public string Grade { get; set; } = grade;
    public double Discount { get; set; } = discount;
    public CappedPoolForecastStatus Status { get; set; } = status;

    /// <summary>Starts a capped pool forecast in status draft.</summary>
    public static CappedPoolForecast NewCappedPoolForecast(string code, string name)
    {
        return new CappedPoolForecast(code, name, 0.0, 0, "", "", 0.0, CappedPoolForecastStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedPoolForecastStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedPoolForecastStatus next)
    {
        switch (Status)
        {
            case CappedPoolForecastStatus.Draft:
                switch (next)
                {
                    case CappedPoolForecastStatus.Pending:
                    case CappedPoolForecastStatus.Suspended:
                        return true;
                }
                return false;
            case CappedPoolForecastStatus.Pending:
                return next == CappedPoolForecastStatus.Active;
            case CappedPoolForecastStatus.Active:
                switch (next)
                {
                    case CappedPoolForecastStatus.Suspended:
                        return true;
                }
                return false;
            case CappedPoolForecastStatus.Suspended:
                return next == CappedPoolForecastStatus.Closed;
        }
        return false;
    }

    public void MoveTo(CappedPoolForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped pool forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"capped pool forecast"} {Code}";
    }

    public double WeightedValue()
    {
        return Math.Round((double)Sequence + Discount, 2);
    }

    public CappedPoolForecast Copy()
    {
        return new CappedPoolForecast(Code, Name, FloorValue, Sequence, ContactEmail, Grade, Discount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
