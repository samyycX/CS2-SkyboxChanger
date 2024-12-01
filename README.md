# CS2-SkyboxChanger
A counterstrikesharp plugin allow player to change their own skybox material, color and brightness on every map dynamically and seamlessly.
![Preview](https://github.com/samyycX/CS2-SkyboxChanger/blob/master/preview.png)

## How to use
1. Download the plugin, decompress it to your counterstrikesharp plugin folder. A config file included default skyboxes will be generated automatically.
2. Edit the config file to add or delete skyboxes.
3. If you are using linux, it might requires to create symlink of `kvlib.so` in `dotnet/shared/Microsoft.NETCore.App/XXX/` folder.

## Permissions
- `@skybox/change` Allow the player to change their own skybox.
- `@skybox/changeall` Allow the player to change all and their own skybox.

## Command
- `!sky / !skybox` Open the select menu. (Need permission)
