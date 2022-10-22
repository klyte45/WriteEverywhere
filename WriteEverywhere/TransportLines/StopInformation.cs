namespace WriteEverywhere.TransportLines
{

    public struct StopInformation
    {
        public ushort m_stopId;
        public bool m_regionalLine;
        public ushort m_lineId;
        public ushort m_nextStopId;
        public ushort m_previousStopId;
        public ushort m_destinationId;
        public string m_destinationString;
        public DestinationType destinationType;

        public override string ToString() => $"StopInformation(S={m_stopId},L={m_lineId}/{(m_regionalLine ? "Building" : "City")},N={m_nextStopId},P={m_previousStopId},D={m_destinationId}(\"{m_destinationString}\" - {destinationType}))";
    }
}