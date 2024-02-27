namespace Shared.Contracts;

public static class TrafficLightConfig
{
    public const string ControlUnitToTrafficLightQueueName = "control_unit_to_traffic_light";
    public const string TrafficLightToControlUnitQueueName = "traffic_light_to_control_unit";
    public const string TrafficLightExchangeName = "traffic_light_exchange";
    public const string ControlUnitToTrafficLightRoutingKey = "control_unit_to_traffic_light_routing_key";
    public const string TrafficLightToControlUnitRoutingKey = "traffic_light_to_control_unit_routing_key";

    public const string Set1Light1Id = "set_1_light_1";
    public const string Set1Light2Id = "set_1_light_2";
    public const string Set2Light1Id = "set_2_light_1";
    public const string Set2Light2Id = "set_2_light_2";
}