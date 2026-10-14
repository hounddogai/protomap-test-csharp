namespace Acme.Workforce.Orgdesign.CostModel.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a cost model reservation is in its life cycle.</summary>
public enum CostModelReservationStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a cost model reservation and its current status.</summary>
public sealed class CostModelReservation(string code, string label, string location, int batchSize, Dictionary<string, string> attributes, double discount, string unit, int endsAt, string salaryBand, int revision, bool active, CostModelReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Location { get; set; } = location;
    public int BatchSize { get; set; } = batchSize;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public double Discount { get; set; } = discount;
    public string Unit { get; set; } = unit;
    public int EndsAt { get; set; } = endsAt;
    public string SalaryBand { get; set; } = salaryBand;
    public int Revision { get; set; } = revision;
    public bool Active { get; set; } = active;
    public CostModelReservationStatus Status { get; set; } = status;

    /// <summary>Starts a cost model reservation in status requested.</summary>
    public static CostModelReservation NewCostModelReservation(string code, string label)
    {
        return new CostModelReservation(
            code,
            label,
            "",
            0,
            new Dictionary<string, string>(),
            0.0,
            "",
            0,
            "",
            0,
            false,
            CostModelReservationStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CostModelReservationStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CostModelReservationStatus next)
    {
        switch (Status)
        {
            case CostModelReservationStatus.Requested:
                switch (next)
                {
                    case CostModelReservationStatus.Granted:
                        return true;
                }
                return false;
            case CostModelReservationStatus.Granted:
                return next == CostModelReservationStatus.Revoked;
            case CostModelReservationStatus.Revoked:
                return next == CostModelReservationStatus.Expired;
        }
        return false;
    }

    public void MoveTo(CostModelReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"cost model reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
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

    public double ProjectedValue()
    {
        return Math.Round((double)BatchSize + (double)Revision, 2);
    }

    public CostModelReservation Copy()
    {
        return new CostModelReservation(
            Code,
            Label,
            Location,
            BatchSize,
            Attributes,
            Discount,
            Unit,
            EndsAt,
            SalaryBand,
            Revision,
            Active,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
