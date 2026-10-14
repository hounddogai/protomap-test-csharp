namespace Acme.Workforce.Assets.LicenseSeat.SeatTemplate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default license seat mapping is in its life cycle.</summary>
public enum DefaultLicenseSeatMappingStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One default license seat mapping, with what the planners need to act on it.</summary>
public sealed class DefaultLicenseSeatMapping(string code, string title, double rate, int batchSize, int quantity, int priority, double floorValue, int revision, double weight, double score, DefaultLicenseSeatMappingStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Rate { get; set; } = rate;
    public int BatchSize { get; set; } = batchSize;
    public int Quantity { get; set; } = quantity;
    public int Priority { get; set; } = priority;
    public double FloorValue { get; set; } = floorValue;
    public int Revision { get; set; } = revision;
    public double Weight { get; set; } = weight;
    public double Score { get; set; } = score;
    public DefaultLicenseSeatMappingStatus Status { get; set; } = status;

    /// <summary>Starts a default license seat mapping in status requested.</summary>
    public static DefaultLicenseSeatMapping NewDefaultLicenseSeatMapping(string code, string title)
    {
        return new DefaultLicenseSeatMapping(
            code,
            title,
            0.0,
            0,
            0,
            0,
            0.0,
            0,
            0.0,
            0.0,
            DefaultLicenseSeatMappingStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultLicenseSeatMappingStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultLicenseSeatMappingStatus next)
    {
        switch (Status)
        {
            case DefaultLicenseSeatMappingStatus.Requested:
                switch (next)
                {
                    case DefaultLicenseSeatMappingStatus.Expired:
                        return true;
                }
                return false;
            case DefaultLicenseSeatMappingStatus.Granted:
                switch (next)
                {
                    case DefaultLicenseSeatMappingStatus.Revoked:
                    case DefaultLicenseSeatMappingStatus.Expired:
                        return true;
                }
                return false;
            case DefaultLicenseSeatMappingStatus.Revoked:
                return next == DefaultLicenseSeatMappingStatus.Expired;
        }
        return false;
    }

    public void MoveTo(DefaultLicenseSeatMappingStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default license seat mapping {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double AdjustedValue()
    {
        return Math.Round(Rate - (double)Quantity, 2);
    }

    public DefaultLicenseSeatMapping Copy()
    {
        return new DefaultLicenseSeatMapping(
            Code,
            Title,
            Rate,
            BatchSize,
            Quantity,
            Priority,
            FloorValue,
            Revision,
            Weight,
            Score,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
