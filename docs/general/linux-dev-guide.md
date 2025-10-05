# Linux Development Cheatsheet - Detailed Guide

## Quick Reference: Daily Commands

**Most frequently used commands for everyday development work:**

```bash
# Navigation & Files
pwd                                         # Show current directory
ls -la                                      # List all files with details
cd /path                                    # Change directory
cat file.txt                                # Display file content
less file.txt                               # View file (scrollable)
tail -f app.log                             # Follow log file in real-time
grep "error" app.log                        # Search text in file
find . -name "*.cs"                         # Find files by pattern

# File Operations
nano file.txt                               # Edit file (simple editor)
cp source dest                              # Copy file
mv old new                                  # Move/rename file
rm file                                     # Delete file
mkdir folder                                # Create directory
chmod +x script.sh                          # Make file executable

# Process Management
ps aux | grep dotnet                        # Find process
kill -9 PID                                 # Force kill process
top                                         # Monitor system resources

# Docker (Daily)
docker ps                                   # List running containers
docker ps -a                                # List all containers
docker logs -f container_id                 # Follow container logs
docker exec -it container_id bash           # Access container shell
docker-compose up -d                        # Start services in background
docker-compose down                         # Stop all services
docker-compose logs -f                      # Follow all logs

# Git (Daily)
git status                                  # Check repository status
git pull                                    # Get latest changes
git add .                                   # Stage all changes
git commit -m "message"                     # Commit changes
git push                                    # Push to remote
git checkout -b feature-branch              # Create new branch

# Services
systemctl status myapp                      # Check service status
systemctl restart myapp                     # Restart service
journalctl -u myapp -f                      # Follow service logs

# PostgreSQL
psql -U postgres -d dbname                  # Connect to database
pg_dump dbname > backup.sql                 # Backup database

# Network
curl http://localhost:5000                  # Test API endpoint
ss -tulnp | grep :5000                      # Check port usage

# System
sudo command                                # Run as admin
df -h                                       # Check disk space
free -h                                     # Check memory usage

# Shortcuts
Ctrl+C                                      # Cancel command
Ctrl+R                                      # Search history
Tab                                         # Auto-complete
!!                                          # Repeat last command
```

---

## Understanding Linux Shells

**What is a Shell?**
A shell is the command-line interface that interprets your commands and communicates with the operating system.

### Common Shells

**Bash (Bourne Again Shell)**
- Default on most Linux systems (Ubuntu, Debian, CentOS)
- What you'll use 90% of the time
- Configuration file: `~/.bashrc`
- Prompt usually ends with `

**Zsh (Z Shell)**
- More features than Bash (better auto-completion, themes)
- Default on macOS (since Catalina)
- Popular with Oh My Zsh framework
- Configuration file: `~/.zshrc`
- Prompt usually ends with `%`

**Fish (Friendly Interactive Shell)**
- Beginner-friendly with syntax highlighting
- Auto-suggestions based on history
- Different syntax from Bash (less compatible)

**Sh (Bourne Shell)**
- Original Unix shell, very basic
- Used mainly for system scripts

### Check Your Current Shell
```bash
echo $SHELL                 # Shows your default shell
echo $0                     # Shows currently running shell
cat /etc/shells             # List all available shells
```

### Switch Between Shells
```bash
bash                        # Start Bash session
zsh                         # Start Zsh session
chsh -s /bin/zsh           # Change default shell to Zsh (restart terminal)
chsh -s /bin/bash          # Change default shell to Bash
```

### Key Differences (Bash vs Zsh)
- **Syntax**: 95% compatible, most commands work the same
- **Auto-completion**: Zsh has better tab completion
- **Customization**: Zsh easier to theme (Oh My Zsh)
- **Scripts**: Bash more universal for shell scripts

### Recommendation for Your Team
**Start with Bash** - It's the default, most documented, and has maximum compatibility. Once comfortable, individuals can explore Zsh for personal productivity.

### Configuration Files
```bash
~/.bashrc                   # Bash: loaded on new terminal
~/.bash_profile             # Bash: loaded on login
~/.zshrc                    # Zsh: main configuration
~/.profile                  # Generic: works for any shell
```

---

## 1. File & Directory Management
**What it is:** The foundation of working in Linux - navigating the file system, creating folders, moving files, and organizing your project structure. Essential for managing your .NET Core projects, configuration files, and deployment scripts.

### Navigation Commands
```bash
pwd                        # Print Working Directory - shows your current location
                          # Example output: /home/user/projects/myapp

ls                        # List files in current directory
ls -l                     # Long format - shows permissions, owner, size, date
ls -a                     # Show all files including hidden files (starting with .)
ls -la                    # Combination: all files with details
ls -lh                    # Human-readable file sizes (KB, MB, GB)
                          # Use this to see configuration files and project structure

cd /path/to/directory     # Change Directory - navigate to specific path
cd ~                      # Go to your home directory (/home/username)
cd ..                     # Go up one level in directory tree
cd ../..                  # Go up two levels
cd -                      # Go back to previous directory
                          # Example: cd /var/www/myapp - navigate to web application folder
```

### Creating & Removing
```bash
mkdir myproject           # Make Directory - create a single folder
mkdir -p parent/child/grandchild  # Create nested directories (parent flag)
                          # -p creates parent directories if they don't exist
                          # Example: mkdir -p /opt/myapp/logs/archive

rmdir directory           # Remove empty directory only
rm filename               # Remove/delete a single file
rm -r directory           # Remove directory recursively (with all contents)
rm -rf directory          # Force remove without confirmation (DANGEROUS - use carefully!)
                          # Example: rm -rf bin/ obj/ - clean build artifacts
rm *.log                  # Remove all .log files in current directory
```

### Copying & Moving
```bash
cp source.txt dest.txt    # Copy file - creates duplicate with new name
cp file.txt /backup/      # Copy file to another directory
cp -r source/ dest/       # Copy directory recursively (all files and subdirs)
cp -p file.txt backup.txt # Preserve permissions and timestamps
                          # Example: cp appsettings.json appsettings.backup.json

mv oldname newname        # Move/rename file (same directory = rename)
mv file.txt /new/location/ # Move file to different directory
mv *.cs /src/Models/      # Move all C# files to Models directory
                          # Example: mv Program.cs Program.old.cs - rename backup

touch newfile.txt         # Create empty file or update timestamp of existing file
                          # Example: touch .gitignore - create git ignore file
```

---

## 2. Viewing & Editing Files
**What it is:** Commands to read log files, configuration files, and source code. Critical for debugging applications, checking logs, and making quick configuration changes without a GUI editor.

### File Content Viewing
```bash
cat file.txt              # Concatenate - display entire file content at once
                          # Good for small files like appsettings.json
                          # Example: cat /etc/hosts - view host mappings

less file.txt             # Paginated viewer - better for large files
                          # Controls: Space=next page, b=previous page, q=quit
                          # /searchterm = search forward, ?searchterm = search backward
                          # Example: less /var/log/syslog - view system logs

more file.txt             # Older alternative to less (less features)

head file.txt             # Show first 10 lines of file
head -n 20 file.txt       # Show first 20 lines
                          # Example: head -n 50 application.log - check recent log entries

tail file.txt             # Show last 10 lines of file
tail -n 20 file.txt       # Show last 20 lines
tail -f app.log           # Follow - real-time log monitoring (Ctrl+C to stop)
tail -f /var/log/myapp/error.log  # Watch error logs in real-time
                          # Critical for debugging running applications!

wc file.txt               # Word Count - lines, words, characters
wc -l file.txt            # Count only lines
                          # Example: wc -l error.log - how many error entries?
```

### Text Editors
```bash
nano file.txt             # Simple text editor (beginner-friendly)
                          # Ctrl+O = save, Ctrl+X = exit
                          # Example: nano appsettings.json - quick config edits

vim file.txt              # Powerful editor (learning curve)
                          # Press 'i' for insert mode, Esc to exit insert mode
                          # :w = save, :q = quit, :wq = save and quit, :q! = quit without saving
                          # Example: vim Program.cs - edit code files

vi file.txt               # Original version of vim (less features)
```

### Searching & Finding
```bash
grep "error" app.log      # Global Regular Expression Print - search for text
grep -i "error" app.log   # Case-insensitive search
grep -n "error" app.log   # Show line numbers
grep -v "debug" app.log   # Invert match - show lines NOT containing "debug"
grep -r "TODO" ./src      # Recursive search in all files under directory
grep -A 5 "error" app.log # Show 5 lines After match
grep -B 5 "error" app.log # Show 5 lines Before match
                          # Example: grep -i "exception" *.log - find all exceptions

find . -name "*.cs"       # Find files by name pattern in current directory
find /var/www -name "web.config"  # Find specific file in path
find . -type f -name "*.dll"      # Find only files (not directories)
find . -type d -name "bin"        # Find only directories
find . -mtime -7          # Find files modified in last 7 days
find . -size +10M         # Find files larger than 10MB
                          # Example: find . -name "*.csproj" - locate all project files

locate filename           # Fast file search using database (needs updatedb first)
```

---

## 3. File Permissions & Ownership
**What it is:** Linux security model - controlling who can read, write, or execute files. Essential for deployment, ensuring your application has correct permissions, and securing configuration files with sensitive data.

### Permission Basics
**Understanding Permissions:**
- `r` = read (4), `w` = write (2), `x` = execute (1)
- Three groups: **Owner** | **Group** | **Others**
- Example: `rwxr-xr--` = Owner can read/write/execute, Group can read/execute, Others can only read

```bash
ls -l                     # View file permissions
                          # Example output: -rw-r--r-- 1 user group 1234 Jan 01 12:00 file.txt
                          # First character: - (file) or d (directory)
                          # Next 9 characters: permissions (rwxrwxrwx)

chmod +x script.sh        # Add execute permission to file (make script runnable)
chmod -x script.sh        # Remove execute permission

chmod 755 script.sh       # Numeric mode: rwxr-xr-x
                          # 7(owner)=rwx, 5(group)=r-x, 5(others)=r-x
                          # Use for scripts that everyone should run

chmod 644 file.txt        # rw-r--r-- - standard file permissions
                          # Owner can read/write, others can only read
                          # Use for configuration files

chmod 600 secrets.json    # rw------- - only owner can read/write
                          # Use for files with sensitive data (passwords, keys)

chmod -R 755 /var/www/myapp  # Recursive - apply to all files/subdirectories
```

### Ownership
```bash
chown user file.txt       # Change file owner
chown user:group file.txt # Change owner and group
chown -R user:group /var/www  # Recursive ownership change
                          # Example: chown www-data:www-data /var/www/myapp
                          # Common for web applications

sudo command              # Super User Do - run command as administrator
                          # Required for system-level operations
                          # Example: sudo systemctl restart myapp.service

sudo su                   # Switch to root user
sudo -u username command  # Run command as different user
```

---

## 4. Process Management
**What it is:** Managing running applications and services. Essential for monitoring your .NET Core applications, killing hung processes, checking resource usage, and managing background jobs.

### Viewing Processes
```bash
ps                        # Process Status - show your processes
ps aux                    # Show ALL processes for all users with details
                          # a=all users, u=user-friendly, x=include non-terminal processes
                          # Columns: USER PID %CPU %MEM VSZ RSS TTY STAT START TIME COMMAND

ps aux | grep dotnet      # Find specific process by name
                          # Example: see all running .NET applications

ps -ef                    # Alternative format showing parent process IDs

top                       # Real-time process monitor (updates every 3 seconds)
                          # Press 'q' to quit, 'k' to kill process, 'M' sort by memory
                          # Shows CPU%, MEM%, running tasks
                          # Use to identify resource-hungry processes

htop                      # Enhanced top (if installed) - colorful, user-friendly
                          # F9=kill process, F6=sort, F5=tree view
```

### Killing Processes
```bash
kill 1234                 # Send SIGTERM (graceful shutdown) to process ID 1234
kill -9 1234              # Send SIGKILL (force kill) - last resort
                          # Use when process is hung and won't respond
kill -15 1234             # SIGTERM (same as kill without number)

killall dotnet            # Kill all processes with name "dotnet"
pkill -f "MyApp.dll"      # Kill processes matching pattern
                          # Example: pkill -f "mywebapp" - stop all instances

# Finding and killing in one go:
ps aux | grep dotnet | awk '{print $2}' | xargs kill -9
# This pipeline: find processes → extract PIDs → kill them
```

### Background Jobs
```bash
command &                 # Run command in background
                          # Example: dotnet run & - run app in background

jobs                      # List background jobs in current session
                          # Shows job number, status, command

fg                        # Bring most recent background job to foreground
fg %1                     # Bring job #1 to foreground

bg                        # Resume suspended job in background

Ctrl+Z                    # Suspend (pause) current foreground process
                          # Process stops but stays in memory

Ctrl+C                    # Interrupt (kill) current foreground process

nohup command &           # No Hangup - keep running after terminal closes
                          # Example: nohup dotnet myapp.dll > output.log 2>&1 &
                          # Output redirected to file, runs even after logout
```

---

## 5. Networking & Connectivity
**What it is:** Commands to test network connections, check which ports your applications are using, make HTTP requests, and troubleshoot connectivity issues. Critical for API development and deployment.

### Testing Connectivity
```bash
ping google.com           # Send ICMP packets to test connectivity
ping -c 4 google.com      # Send only 4 packets then stop
                          # Example: ping database-server - test DB connectivity

curl http://localhost:5000  # Make HTTP request and display response
curl -i http://api.com      # Include response headers
curl -X POST http://api.com # Make POST request
curl -X POST -H "Content-Type: application/json" \
  -d '{"key":"value"}' http://api.com/endpoint
                          # Test your REST APIs
curl -o file.txt http://example.com/file  # Save response to file

wget https://example.com/file.zip  # Download file from URL
wget -c url               # Continue interrupted download
wget -r url               # Recursive download (entire website)
```

### Port & Network Information
```bash
netstat -tulnp            # Show listening ports and established connections
                          # t=TCP, u=UDP, l=listening, n=numeric, p=programs
                          # Example: find which app is using port 5000

ss -tulnp                 # Socket Statistics - modern netstat replacement (faster)
ss -tulnp | grep :5000    # Check specific port
ss -tan                   # Show TCP connections

lsof -i :5000             # List Open Files - what's using port 5000?
lsof -i -P                # Show all network connections
                          # Example: lsof -i :80 - see what's on web port

netstat -an | grep LISTEN # Show all listening ports
```

### Network Interface Information
```bash
ifconfig                  # Show network interfaces and IP addresses
ifconfig eth0             # Show specific interface
                          # Example output: inet 192.168.1.10 netmask 255.255.255.0

ip addr                   # Modern replacement for ifconfig
ip addr show eth0         # Show specific interface
ip link                   # Show network devices

hostname                  # Show system hostname
hostname -I               # Show all IP addresses

nslookup google.com       # DNS lookup - get IP from domain name
dig google.com            # Detailed DNS information
```

---

## 6. Environment Variables
**What it is:** System-wide and user-specific variables that configure application behavior. Essential for .NET Core configuration (ConnectionStrings, API keys, environment settings) without hardcoding values.

### Working with Variables
```bash
export MY_VAR="value"     # Set environment variable for current session
export PATH=$PATH:/new/path  # Add to existing PATH variable
                          # Example: export ASPNETCORE_ENVIRONMENT="Production"
                          # This tells .NET Core which environment to use

echo $MY_VAR              # Display variable value ($ accesses the variable)
echo $HOME                # Your home directory path
echo $USER                # Current username
echo $PATH                # Executable search paths
echo $SHELL               # Current shell program

env                       # List all environment variables
printenv                  # Same as env
printenv PATH             # Show specific variable

set                       # Show all variables including shell variables

unset MY_VAR              # Remove environment variable from current session
```

### Configuration Files
```bash
~/.bashrc                 # User's bash configuration (loaded on new terminal)
~/.bash_profile           # Loaded on login
/etc/environment          # System-wide environment variables

# Editing .bashrc for permanent variables:
nano ~/.bashrc
# Add: export CONNECTION_STRING="Server=localhost;Database=mydb;"
source ~/.bashrc          # Reload configuration immediately
                          # Or close and reopen terminal

# For .NET Core applications:
export ASPNETCORE_URLS="http://localhost:5000"
export ConnectionStrings__DefaultConnection="Host=localhost;Database=mydb"
                          # Double underscore (__) for nested configuration
```

---

## 7. Package Management (Ubuntu/Debian)
**What it is:** Installing and managing software packages. Essential for installing development tools, database clients, libraries, and dependencies for your .NET Core applications.

### APT (Advanced Package Tool)
```bash
sudo apt update           # Update package list from repositories
                          # Always run this first before installing anything
                          # Downloads latest package information

sudo apt upgrade          # Upgrade all installed packages to latest versions
sudo apt upgrade -y       # Auto-confirm all prompts

sudo apt install package-name     # Install specific package
sudo apt install postgresql-client  # Example: install PostgreSQL client tools
sudo apt install -y nginx         # Install with auto-confirm

sudo apt remove package-name      # Remove package but keep configuration
sudo apt purge package-name       # Remove package and configuration files
sudo apt autoremove               # Remove unused dependencies

sudo apt search keyword   # Search for packages
                          # Example: apt search postgres - find PostgreSQL packages

apt list --installed      # List all installed packages
apt list --upgradable     # List packages that can be upgraded

sudo apt-cache policy package  # Show available versions
sudo apt show package     # Show detailed package information
```

### Package Information
```bash
dpkg -l                   # List all installed packages with versions
dpkg -l | grep postgres   # Find specific installed packages
dpkg -L package-name      # List files installed by package
dpkg -s package-name      # Show package status and details

which command             # Show path to executable
                          # Example: which dotnet - find .NET installation
whereis command           # Show binary, source, and manual paths
```

---

## 8. System Information
**What it is:** Commands to check system resources, disk space, memory usage, and system details. Important for monitoring application performance and diagnosing resource issues.

### System Details
```bash
uname -a                  # Show all system information
                          # Kernel version, hostname, architecture
uname -r                  # Kernel version only
uname -m                  # Architecture (x86_64, arm64, etc.)

cat /etc/os-release       # Detailed OS information (Ubuntu version, etc.)
lsb_release -a            # Distribution information

date                      # Current date and time
uptime                    # How long system has been running + load average
                          # Load average: 1min, 5min, 15min averages

whoami                    # Current username
hostname                  # System hostname
hostname -I               # System IP address(es)
```

### Disk Usage
```bash
df -h                     # Disk Free - show disk space (human-readable)
                          # Shows: filesystem, size, used, available, use%, mount point
df -h /var/www            # Check specific path

du -sh directory/         # Disk Usage - show directory size
du -sh *                  # Size of all items in current directory
du -h --max-depth=1       # Show size of subdirectories one level deep
du -h | sort -h           # Sort by size
                          # Example: du -sh /var/log/* - check log sizes

ncdu                      # Interactive disk usage analyzer (if installed)
                          # Visual, easy to navigate
```

### Memory & CPU
```bash
free -h                   # Show memory usage (RAM and swap)
                          # Total, used, free, shared, cached
                          # Example: see if app is consuming too much memory

cat /proc/cpuinfo         # CPU information (cores, model, speed)
lscpu                     # CPU architecture information
nproc                     # Number of CPU cores

cat /proc/meminfo         # Detailed memory information
vmstat 1                  # Virtual memory statistics every 1 second
iostat                    # CPU and I/O statistics (if sysstat installed)
```

---

## 9. Service Management (systemd)
**What it is:** Modern Linux service manager. Used to start/stop your .NET Core applications as services, enable auto-start on boot, and view service logs. Critical for production deployments.

### Service Control
```bash
systemctl status servicename    # Check service status (running, stopped, failed)
                                # Shows: active/inactive, PID, memory, recent logs
                                # Example: systemctl status postgresql

systemctl start servicename     # Start a service
systemctl stop servicename      # Stop a service
systemctl restart servicename   # Stop then start service
systemctl reload servicename    # Reload configuration without stopping
                                # Example: systemctl restart myapp.service

systemctl enable servicename    # Enable service to start on boot
systemctl disable servicename   # Disable auto-start on boot
systemctl is-enabled servicename  # Check if enabled

systemctl list-units --type=service  # List all services
systemctl list-units --type=service --state=running  # Running services only
systemctl list-units --type=service --state=failed   # Failed services
```

### Service Logs (journalctl)
```bash
journalctl                      # View all system logs
journalctl -u servicename       # View logs for specific service
                                # Example: journalctl -u myapp.service

journalctl -f                   # Follow logs in real-time (like tail -f)
journalctl -u myapp -f          # Follow specific service logs

journalctl -u myapp --since today      # Today's logs only
journalctl -u myapp --since "1 hour ago"  # Last hour
journalctl -u myapp --since "2024-01-01"  # From specific date

journalctl -u myapp -n 50       # Show last 50 lines
journalctl -u myapp -p err      # Show only errors
                                # Priorities: emerg, alert, crit, err, warning, notice, info, debug

journalctl --disk-usage         # Check log disk usage
journalctl --vacuum-time=7d     # Delete logs older than 7 days
```

### Creating a .NET Core Service
```bash
# Example service file: /etc/systemd/system/myapp.service
[Unit]
Description=My .NET Core Application
After=network.target

[Service]
WorkingDirectory=/var/www/myapp
ExecStart=/usr/bin/dotnet /var/www/myapp/MyApp.dll
Restart=always
RestartSec=10
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production

[Install]
WantedBy=multi-user.target

# After creating service file:
sudo systemctl daemon-reload    # Reload systemd configuration
sudo systemctl enable myapp     # Enable on boot
sudo systemctl start myapp      # Start the service
```

---

## 10. Docker Commands
**What it is:** Container platform for packaging and running applications. Essential for deploying .NET Core apps consistently across environments, running PostgreSQL locally, and managing microservices.

### Image Management
```bash
docker images               # List all images on your system
docker images -a            # Include intermediate images

docker build -t myapp:latest .          # Build image from Dockerfile in current directory
docker build -t myapp:v1.0 -f Dockerfile.prod .  # Specify Dockerfile
                            # -t = tag (name:version)
                            # Example: docker build -t mywebapi:latest .

docker pull postgres:15     # Download image from Docker Hub
docker pull mcr.microsoft.com/dotnet/aspnet:8.0  # Pull .NET runtime

docker tag myapp:latest myapp:v1.0      # Create new tag for existing image
docker rmi image_id         # Remove image
docker rmi $(docker images -q)  # Remove all images (careful!)
```

### Container Management
```bash
docker ps                   # List running containers
docker ps -a                # List all containers (including stopped)
docker ps -a --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"  # Custom format

docker run myapp            # Create and start container from image
docker run -d myapp         # Run in detached mode (background)
docker run --name mycontainer myapp  # Assign name to container
docker run -p 5000:80 myapp # Port mapping: host:container
                            # Example: docker run -d -p 5432:5432 postgres:15

docker run -e "VAR=value" myapp  # Set environment variable
docker run -v /host/path:/container/path myapp  # Volume mount
                            # Example: -v $(pwd)/data:/var/lib/postgresql/data

docker start container_id   # Start stopped container
docker stop container_id    # Stop running container (SIGTERM)
docker kill container_id    # Force stop (SIGKILL)
docker restart container_id # Restart container

docker rm container_id      # Remove stopped container
docker rm -f container_id   # Force remove running container
docker rm $(docker ps -aq)  # Remove all containers
```

### Container Interaction
```bash
docker exec -it container_id bash       # Access container shell interactively
                                        # -i=interactive, -t=pseudo-TTY
docker exec container_id ls /app        # Run command in container
                                        # Example: docker exec myapp ls /var/www

docker logs container_id                # View container logs
docker logs -f container_id             # Follow logs in real-time
docker logs --tail 100 container_id     # Last 100 lines
docker logs --since 30m container_id    # Logs from last 30 minutes

docker inspect container_id             # Detailed container information (JSON)
docker stats                            # Real-time resource usage (CPU, memory)
docker top container_id                 # Running processes in container
```

### Docker Compose
```bash
docker-compose up           # Start all services defined in docker-compose.yml
docker-compose up -d        # Start in background
docker-compose up --build   # Rebuild images before starting

docker-compose down         # Stop and remove containers
docker-compose down -v      # Also remove volumes

docker-compose ps           # List containers
docker-compose logs         # View logs from all services
docker-compose logs -f myapp  # Follow logs for specific service

docker-compose restart      # Restart all services
docker-compose restart myapp  # Restart specific service

docker-compose exec myapp bash  # Access service container
```

### Cleanup
```bash
docker system df            # Show disk usage
docker system prune         # Remove unused data (stopped containers, unused networks, dangling images)
docker system prune -a      # Remove all unused images (not just dangling)
docker volume prune         # Remove unused volumes
docker network prune        # Remove unused networks
```

---

## 11. PostgreSQL Commands
**What it is:** Database interaction commands. Essential for managing your PostgreSQL databases, running queries, backing up data, and troubleshooting database issues during migration.

### Connecting to PostgreSQL
```bash
psql                        # Connect to default database as current user
psql -U username            # Connect as specific user
psql -U postgres            # Connect as postgres superuser
psql -h localhost -U postgres  # Specify host
psql -h hostname -p 5432 -U user -d database  # Full connection
                            # -h=host, -p=port, -U=user, -d=database

psql "postgresql://user:password@localhost:5432/dbname"  # Connection string

# After connecting, you're in psql prompt (shows: database=#)
```

### psql Meta-commands (inside psql)
```bash
\l                          # List all databases
\l+                         # List with sizes and descriptions

\c database_name            # Connect to different database
\c myapp postgres           # Connect to myapp as user postgres

\dt                         # List tables in current database
\dt+                        # List with sizes and descriptions
\dt schema.*                # List tables in specific schema

\d table_name               # Describe table structure (columns, types, constraints)
\d+ table_name              # Detailed description with indexes and triggers

\du                         # List database users/roles
\dn                         # List schemas
\df                         # List functions
\dv                         # List views

\x                          # Toggle expanded display (vertical)
                            # Useful for wide result sets

\timing                     # Toggle query execution timing
\e                          # Open external editor for query
\q                          # Quit psql
\?                          # Help on meta-commands
\h SQL_COMMAND              # Help on SQL commands
                            # Example: \h SELECT
```

### Database Operations
```bash
# Create database
psql -U postgres -c "CREATE DATABASE myapp;"

# Create user
psql -U postgres -c "CREATE USER myuser WITH PASSWORD 'mypassword';"

# Grant privileges
psql -U postgres -c "GRANT ALL PRIVILEGES ON DATABASE myapp TO myuser;"

# Execute SQL file
psql -U postgres -d myapp -f schema.sql

# Execute single query
psql -U postgres -d myapp -c "SELECT COUNT(*) FROM users;"
```

### Backup & Restore
```bash
# Backup single database
pg_dump dbname > backup.sql
pg_dump -U postgres myapp > myapp_backup.sql
pg_dump -U postgres -h localhost myapp > myapp_$(date +%Y%m%d).sql

# Backup with custom format (compressed, can restore selectively)
pg_dump -Fc dbname > backup.dump

# Backup all databases
pg_dumpall -U postgres > all_databases.sql

# Backup only schema (no data)
pg_dump --schema-only dbname > schema.sql

# Backup only data (no schema)
pg_dump --data-only dbname > data.sql

# Restore database
psql dbname < backup.sql
psql -U postgres myapp < myapp_backup.sql

# Restore custom format
pg_restore -d dbname backup.dump
pg_restore -U postgres -d myapp -c backup.dump  # -c drops objects before recreating

# Restore specific table
pg_restore -d dbname -t tablename backup.dump
```

### Monitoring & Troubleshooting
```bash
# Check PostgreSQL is running
systemctl status postgresql
ps aux | grep postgres

# View connections
psql -U postgres -c "SELECT * FROM pg_stat_activity;"

# Kill long-running query
psql -U postgres -c "SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE pid = 12345;"

# Check database size
psql -U postgres -c "\l+"

# Check table sizes
psql -U postgres -d myapp -c "
SELECT tablename, pg_size_pretty(pg_total_relation_size(schemaname||'.'||tablename))
FROM pg_tables WHERE schemaname = 'public' ORDER BY pg_total_relation_size(schemaname||'.'||tablename) DESC;"
```

---

## 12. Git Commands (Quick Reference)
**What it is:** Version control system. Essential for collaborative development, tracking code changes, and deploying applications. Every developer needs these commands daily.

### Basic Workflow
```bash
git init                    # Initialize new repository in current directory
git clone url               # Clone remote repository
                            # Example: git clone https://github.com/user/repo.git

git status                  # Show working tree status (modified files, staged files)
git status -s               # Short format

git add file.txt            # Stage specific file for commit
git add .                   # Stage all changes in current directory
git add *.cs                # Stage all C# files

git commit -m "message"     # Commit staged changes with message
git commit -am "message"    # Stage and commit modified files (not new files)

git push                    # Push commits to remote repository
git push origin main        # Push to specific branch
git push -u origin feature  # Push and set upstream branch

git pull                    # Fetch and merge changes from remote
git pull origin main        # Pull from specific branch

git fetch                   # Download changes without merging
```

### Branching
```bash
git branch                  # List local branches (* shows current)
git branch -a               # List all branches (including remote)
git branch -r               # List remote branches

git branch feature-name     # Create new branch
git checkout feature-name   # Switch to branch
git checkout -b feature-name  # Create and switch to new branch
git switch feature-name     # Modern alternative to checkout

git merge feature-name      # Merge branch into current branch
git branch -d feature-name  # Delete branch (safe - won't delete unmerged)
git branch -D feature-name  # Force delete branch

git push origin --delete feature  # Delete remote branch
```

### History & Changes
```bash
git log                     # Show commit history
git log --oneline           # Compact one-line format
git log --graph --oneline   # Visual branch graph
git log -n 5                # Last 5 commits
git log --since="2 weeks ago"  # Commits from last 2 weeks

git diff                    # Show unstaged changes
git diff --staged           # Show staged changes
git diff branch1..branch2   # Compare branches

git show commit_hash        # Show commit details
git blame file.txt          # Show who modified each line
```

### Undoing Changes
```bash
git checkout -- file.txt    # Discard changes in working directory
git restore file.txt        # Modern alternative

git reset file.txt          # Unstage file (keep changes)
git reset --soft HEAD~1     # Undo last commit (keep changes staged)
git reset --hard HEAD~1     # Undo last commit (discard all changes - dangerous!)

git revert commit_hash      # Create new commit that undoes changes
```

---

## 13. Text Processing & Piping
**What it is:** Combining commands to filter, transform, and analyze text data. Powerful for processing logs, extracting information, and automating tasks.

### Piping Basics
```bash
command1 | command2         # Pipe: send output of command1 to command2
                            # Example: cat log.txt | grep "error"

cat file.txt | grep "error" | wc -l  # Count lines containing "error"

ps aux | grep dotnet        # Find .NET processes
ps aux | grep dotnet | grep -v grep  # Exclude grep itself from results

ls -l | sort -k5 -n         # Sort files by size (5th column, numeric)
```

### Redirection
```bash
command > file.txt          # Redirect output to file (overwrite)
command >> file.txt         # Append output to file
command 2> error.log        # Redirect error output (stderr)
command > output.log 2>&1   # Redirect both stdout and stderr to same file
                            # 2>&1 means "redirect stderr to wherever stdout goes"

command < input.txt         # Use file as input
```

### Text Processing Tools
```bash
awk '{print $1}' file.txt   # Print first column
ps aux | awk '{print $2, $11}'  # Extract PID and command name

sed 's/old/new/g' file.txt  # Replace text (stream editor)
sed -i 's/old/new/g' file   # Replace in-place (modify file)

cut -d',' -f1,3 data.csv    # Cut columns 1 and 3 from CSV
                            # -d=',' delimiter, -f=fields

sort file.txt               # Sort lines
sort -r file.txt            # Reverse sort
sort -n file.txt            # Numeric sort
sort -u file.txt            # Sort and remove duplicates

uniq                        # Remove duplicate lines (requires sorted input)
sort file.txt | uniq -c     # Count occurrences

tr 'a-z' 'A-Z' < file.txt   # Translate lowercase to uppercase
```

---

## 14. Compression & Archives
**What it is:** Creating and extracting compressed files. Essential for deployment packages, backups, and transferring files efficiently.

### tar Archives
```bash
tar -czf archive.tar.gz folder/     # Create compressed archive
                                    # -c=create, -z=gzip, -f=file
tar -xzf archive.tar.gz             # Extract archive
                                    # -x=extract, -z=gzip, -f=file
tar -xzf archive.tar.gz -C /dest/   # Extract to specific directory

tar -tzf archive.tar.gz             # List contents without extracting
                                    # -t=list

# Exclude files while creating
tar -czf archive.tar.gz --exclude='*.log' --exclude='bin/' folder/
```

### zip Archives
```bash
zip archive.zip file.txt            # Create zip with single file
zip -r archive.zip folder/          # Create zip recursively
                                    # -r=recursive (include subdirectories)

unzip archive.zip                   # Extract zip
unzip archive.zip -d /destination/  # Extract to specific directory
unzip -l archive.zip                # List contents

# Create deployment package
zip -r deploy.zip . -x "*.git*" "bin/*" "obj/*"
```

---

## 15. Useful Shortcuts & Tips
**What it is:** Keyboard shortcuts and command-line tricks to work faster and more efficiently.

### Terminal Shortcuts
```bash
Ctrl+C              # Cancel/interrupt current command
Ctrl+Z              # Suspend current process (can resume with fg/bg)
Ctrl+D              # Exit/logout (send EOF signal)
Ctrl+L              # Clear screen (same as 'clear' command)
Ctrl+A              # Move cursor to start of line
Ctrl+E              # Move cursor to end of line
Ctrl+U              # Delete from cursor to start of line
Ctrl+K              # Delete from cursor to end of line
Ctrl+W              # Delete word before cursor
Ctrl+R              # Search command history (start typing, press again for previous)
                    # Press Enter to run, or arrow keys to edit

Tab                 # Auto-complete command, file, or directory name
                    # Press twice to see all options

Up/Down arrows      # Navigate command history
!!                  # Repeat last command
!n                  # Repeat command number n from history
                    # Use 'history' to see numbered list
!$                  # Use last argument from previous command
                    # Example: mkdir newdir; cd !$ (cd to newdir)

Ctrl+Shift+C        # Copy selected text
Ctrl+Shift+V        # Paste text in terminal
```

### Command Combinations
```bash
command1 && command2    # Run command2 only if command1 succeeds
                        # Example: make && make install

command1 || command2    # Run command2 only if command1 fails
                        # Example: dotnet build || echo "Build failed"

command1; command2      # Run both commands regardless of success/failure

cd /var/www && git pull # Navigate and pull in one line
```

### Aliases (Custom Shortcuts)
```bash
# Add to ~/.bashrc for permanent aliases
alias ll='ls -la'       # Create shortcut
alias gs='git status'
alias dc='docker-compose'
alias logs='journalctl -f'

# Apply aliases
source ~/.bashrc

# View all aliases
alias

# Remove alias
unalias ll
```

---

## 16. File Transfer
**What it is:** Copying files between local and remote systems. Essential for deploying code, transferring backups, and managing files on servers.

### SCP (Secure Copy)
```bash
# Copy local file to remote server
scp file.txt user@server:/path/to/destination/
scp myapp.zip admin@192.168.1.100:/var/www/

# Copy remote file to local
scp user@server:/path/to/file.txt .
scp user@server:/path/to/file.txt /local/destination/

# Copy directory recursively
scp -r folder/ user@server:/destination/

# Copy with specific port
scp -P 2222 file.txt user@server:/path/

# Copy with compression (faster for large files)
scp -C file.txt user@server:/path/
```

### rsync (Advanced Sync)
```bash
# Sync directory to remote (more efficient than scp)
rsync -avz source/ user@server:/destination/
# -a=archive mode (recursive, preserve permissions, timestamps)
# -v=verbose
# -z=compress during transfer

# Sync with delete (mirror exact copy)
rsync -avz --delete source/ user@server:/destination/

# Dry run (preview changes without actually copying)
rsync -avz --dry-run source/ destination/

# Exclude patterns
rsync -avz --exclude='*.log' --exclude='node_modules/' source/ dest/

# Show progress
rsync -avz --progress source/ destination/
```

---

## 17. Monitoring & Debugging
**What it is:** Advanced commands for troubleshooting system issues, monitoring performance, and debugging applications.

### System Monitoring
```bash
strace command              # Trace system calls made by program
                            # Example: strace dotnet myapp.dll
                            # Shows: file opens, network calls, errors

ltrace command              # Trace library calls

watch -n 2 "command"        # Run command every 2 seconds
                            # Example: watch -n 5 "docker ps"
                            # Ctrl+C to stop

dmesg                       # View kernel ring buffer (hardware/driver messages)
dmesg | tail                # Recent kernel messages
dmesg | grep -i error       # Kernel errors

iostat                      # CPU and I/O statistics (if sysstat installed)
iostat -x 2                 # Extended stats every 2 seconds

vmstat 2                    # Virtual memory statistics every 2 seconds
mpstat                      # Multi-processor statistics

sar                         # System Activity Report (historical data)
```

### Network Monitoring
```bash
tcpdump -i eth0             # Capture network packets on interface
                            # Requires root/sudo
tcpdump -i any port 80      # Capture HTTP traffic
tcpdump -i any host 192.168.1.1  # Capture traffic to/from specific host

iftop                       # Real-time network bandwidth usage
nethogs                     # Network usage per process
nload                       # Network load visualization
```

---

## 18. Getting Help
**What it is:** Built-in documentation and help resources. Use these when you need more details about any command.

```bash
man command                 # Manual page for command (detailed documentation)
                            # Example: man grep
                            # Press q to quit, / to search

command --help              # Quick help summary
command -h                  # Alternative short form

info command                # Info documentation (alternative to man)

whatis command              # One-line description of command

apropos keyword             # Search man pages for keyword
                            # Example: apropos network - find network-related commands

type command                # Show command type (alias, function, or executable)
which command               # Show full path to executable
```

---

## Pro Tips for Development

1. **Tab Completion**: Always use Tab to autocomplete commands, files, and directories
2. **History Search**: Use Ctrl+R to search your command history - huge time saver
3. **Aliases**: Create shortcuts for frequently used commands in ~/.bashrc
4. **Screen/tmux**: Use terminal multiplexers to keep sessions running after disconnect
5. **Logs**: Always check logs when something fails (journalctl, docker logs, application logs)
6. **Permissions**: 755 for scripts, 644 for files, 600 for sensitive data
7. **Backup**: Always backup before major changes (pg_dump, tar, git commit)
8. **Documentation**: Use man pages and --help when unsure about options