namespace Acme.Workforce.Compensation.EquityGrant.Plan;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched equity grant preference is in its life cycle.</summary>
public enum BatchedEquityGrantPreferenceStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a batched equity grant preference and its current status.</summary>
public sealed class BatchedEquityGrantPreference(string code, string name, string team, double floorValue, double rating, int batchSize, BatchedEquityGrantPreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Team { get; set; } = team;
    public double FloorValue { get; set; } = floorValue;
    public double Rating { get; set; } = rating;
    public int BatchSize { get; set; } = batchSize;
    public BatchedEquityGrantPreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a batched equity grant preference in status created.</summary>
    public static BatchedEquityGrantPreference NewBatchedEquityGrantPreference(string code, string name)
    {
        return new BatchedEquityGrantPreference(
            code,
            name,
            "",
            0.0,
            0.0,
            0,
            BatchedEquityGrantPreferenceStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedEquityGrantPreferenceStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedEquityGrantPreferenceStatus next)
    {
        switch (Status)
        {
            case BatchedEquityGrantPreferenceStatus.Created:
                switch (next)
                {
                    case BatchedEquityGrantPreferenceStatus.Validated:
                    case BatchedEquityGrantPreferenceStatus.Published:
                        return true;
                }
                return false;
            case BatchedEquityGrantPreferenceStatus.Validated:
                switch (next)
                {
                    case BatchedEquityGrantPreferenceStatus.Published:
                    case BatchedEquityGrantPreferenceStatus.Retired:
                        return true;
                }
                return false;
            case BatchedEquityGrantPreferenceStatus.Published:
                switch (next)
                {
                    case BatchedEquityGrantPreferenceStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BatchedEquityGrantPreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched equity grant preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double WeightedLoad()
    {
        return Math.Round(FloorValue + Rating, 2);
    }

    public BatchedEquityGrantPreference Copy()
    {
        return new BatchedEquityGrantPreference(Code, Name, Team, FloorValue, Rating, BatchSize, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
