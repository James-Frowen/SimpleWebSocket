## Usage

To create a plugin for MirrorNG, follow these steps:

1) Click on "Use this template" button at the top right to create your repo
2) Rename Assets/MyPlugin with a name appropriate for your plugin
3) Rename MyPlugin.asmdef
4) edit MyPlugin.asmdef and change the name of the assembly
5) adjust package.json to describe your project and change the package name
6) update .releaserc.yml with the correct path
7) update README.txt to describe your project
8) While not strictly required,  I suggest creating a symbolic link from Assets/MyPlugin/Samples~ to Assets/Samples. This will allow you to open and edit your examples in unity.
9) Add your samples to package.json, so upm can install them.
10) If you create the plugin in MirrorNG repo,  you will automatically use the org license for unity
   otherwise activate a manual license and add it as a secret in your repo called UNITY_LICENSE. see https://github.com/MirrorNG/unity-runner for detailed instructions
11) Create a sonar qube project for your repo in [sonarcloud](https://sonarcloud.io) or ask Paul if you would like it created in the MirrorNG org
12) add SONAR_PROJECT_KEY secret.  It must be set to the project id in sonarcloud.  For example for MirrorNG it is MirrorNG_MirrorNG
13) Add SONAR_PROJECT_NAME secret.  Set it to a human readable name for your project
14) add SONAR_TOKEN secret. You can get the token by going through the configuration wizard in sonar qube
15) Replace this readme with README.md.sample and edit as appropriate

Once your plugin is working the way you like,  you can add it to openupm by going to https://openupm.com/packages/add/
