import re

version_regex = r"Version>(\d+\.\d+\.\d+)<"

with open("Server/Server.csproj") as f:
    text = f.read()

version = re.findall(version_regex, text)
version = "1.0.0" if not version else version[0]
print(version)
