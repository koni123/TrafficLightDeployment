namespace Shared.Contracts;

public static class TrafficLightConfig
{
    public const string ControlUnitToTrafficLightQueueName = "control_unit_to_traffic_light";
    public const string TrafficLightToControlUnitQueueName = "traffic_light_to_control_unit";
    public const string TrafficLightExchangeName = "traffic_light_exchange";
    public const string ControlUnitToTrafficLightRoutingKey = "control_unit_to_traffic_light_routing_key";
    public const string TrafficLightToControlUnitRoutingKey = "traffic_light_to_control_unit_routing_key";

    public const string TrafficLight1Id = "traffic_light_id_1";
    public const string TrafficLight2Id = "traffic_light_id_2";
    public const string TrafficLight3Id = "traffic_light_id_3";
    public const string TrafficLight4Id = "traffic_light_id_4";
}