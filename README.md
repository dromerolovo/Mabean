
## Installation

### 1. Install Git

Make sure Git is installed on your system.


### 2. (Optional) Install msfvenom

Install **msfvenom** if you plan to generate payloads or experiment with reverse shells.
It should work when running the command in a shell

```bash
msfvenom
```

### 3. Clone the repository
```bash
git clone https://github.com/dromerolovo/Mabean.git
```

### 4. Navigate to the root of the repo 
```bash
cd Mabean
```

### 5. Disable Windows Defender
Windows Security -> Virus & Thread Protection -> Manage Settings -> Turn off real time protection

### 6. Run the environment installation script
```bash
.\EnvironmentInstallation.ps1
```

### 7. Run Mabean Installation script
```bash
.\MabeanInstall.ps1 -geminiApiKey <your gemini api key>
```

### 8. Run the executable 
You'll find an executable in the Desktop, run it
