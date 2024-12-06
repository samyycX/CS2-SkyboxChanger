using System.Text.Json.Serialization;
using CounterStrikeSharp.API.Core;

public class Skybox
{
  [JsonPropertyName("name")]
  public string Name { get; set; } = "";

  [JsonPropertyName("material")]
  public string Material { get; set; } = "";
  [JsonPropertyName("brightness")]
  public float? Brightness { get; set; } = null;

  [JsonPropertyName("color")]
  public string? Color { get; set; } = null;

  [JsonPropertyName("permissions")]
  public string[]? Permissions { get; set; } = null;
  [JsonPropertyName("permissionsOr")]
  public string[]? PermissionsOr { get; set; } = null;

}

public class DatabaseConfig
{

  public string Host { get; set; } = "127.0.0.1";

  public int Port { get; set; } = 3306;
  public string User { get; set; } = "root";

  public string Password { get; set; } = "";

  public string Database { get; set; } = "cs2";

  public string TablePrefix { get; set; } = "cs2_skyboxchanger_";
}

public class SkyboxConfig : BasePluginConfig
{
  [JsonPropertyName("Skybox")]
  public Dictionary<string, Skybox> Skyboxs { get; set; } = new() {
    { "cs_italy_s2_skybox_2", new Skybox { Name = "cs_italy_s2_skybox_2", Material = "materials/skybox/cs_italy_s2_skybox_2.vmat" } },
    { "cs_italy_s2_skybox_2_fog", new Skybox { Name = "cs_italy_s2_skybox_2_fog", Material = "materials/skybox/cs_italy_s2_skybox_2_fog.vmat" } },
    { "cs_italy_s2_skybox_2_lightning", new Skybox { Name = "cs_italy_s2_skybox_2_lightning", Material = "materials/skybox/cs_italy_s2_skybox_2_lighting.vmat" } },
    { "cs_office_45_0", new Skybox { Name = "cs_office_45_0", Material = "materials/skybox/cs_office_45_0.vmat" } },
    { "sky_ar_baggage_01", new Skybox { Name = "sky_ar_baggage_01", Material = "materials/skybox/sky_ar_baggage_01.vmat" } },
    { "sky_black", new Skybox { Name = "sky_black", Material = "materials/skybox/sky_black.vmat" } },
    { "sky_csgo_cloudy01", new Skybox { Name = "sky_csgo_cloudy01", Material = "materials/skybox/sky_csgo_cloudy01.vmat" } },
    { "sky_de_annubis", new Skybox { Name = "sky_de_annubis", Material = "materials/skybox/sky_de_annubis.vmat" } },
    { "sky_de_dust2", new Skybox { Name = "sky_de_dust2", Material = "materials/skybox/sky_de_dust2.vmat" } },
    { "sky_de_mirage", new Skybox { Name = "sky_de_mirage", Material = "materials/skybox/sky_de_mirage.vmat" } },
    { "sky_de_nuke", new Skybox { Name = "sky_de_nuke", Material = "materials/skybox/sky_de_nuke.vmat" } },
    { "sky_de_overpass_01", new Skybox { Name = "sky_de_overpass_01", Material = "materials/skybox/sky_de_overpass_01.vmat" } },
    { "sky_de_train03", new Skybox { Name = "sky_de_train03", Material = "materials/skybox/sky_de_train03.vmat" } },
    { "sky_de_vertigo", new Skybox { Name = "sky_de_vertigo", Material = "materials/skybox/sky_de_vertigo.vmat" } },
    { "sky_hr_aztec_02", new Skybox { Name = "sky_hr_aztec_02", Material = "materials/skybox/sky_hr_aztec_02.vmat" } },
    { "sky_hr_aztec_02_lighting", new Skybox { Name = "sky_hr_aztec_02_lighting", Material = "materials/skybox/sky_hr_aztec_02_lighting.vmat" } },
    { "sky_overcast_01", new Skybox { Name = "sky_overcast_01", Material = "materials/skybox/sky_overcast_01.vmat" } },
  };

  [JsonPropertyName("Database")]
  public DatabaseConfig Database { get; set; } = new();

  [JsonPropertyName("MapDefault")]
  public Dictionary<string, string>? MapDefault { get; set; } = new();


  [JsonPropertyName("Version")]
  public override int Version { get; set; } = 2;
}