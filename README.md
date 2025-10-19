# Network Speed Monitor

A native Windows desktop application that displays real-time network download and upload speeds, similar to NetSpeedMonitor.

## Features

- **Real-time monitoring**: Updates network speeds every second
- **System Tray Icon**: Displays speeds directly in the taskbar notification area
- **Taskbar Widget**: Optional always-on-top overlay window positioned near the system tray
- **Download and Upload speeds**: Shows current transfer rates in both window and tray icon
- **Total data tracking**: Shows cumulative downloaded and uploaded data
- **Interface selection**: Choose which network adapter to monitor
- **Modern UI**: Clean, dark-themed interface with color-coded metrics
- **Minimize to tray**: Window minimizes to system tray instead of taskbar
- **Draggable Widget**: Reposition the taskbar widget anywhere on your screen

## Requirements

- Windows OS
- .NET 8.0 Runtime or later

## Building the Application

1. Make sure you have .NET 8.0 SDK installed:
   ```powershell
   dotnet --version
   ```

2. Build the application:
   ```powershell
   dotnet build
   ```

3. Run the application:
   ```powershell
   dotnet run
   ```

## Creating an Executable

To create a standalone executable:

```powershell
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

The executable will be in: `bin\Release\net8.0-windows\win-x64\publish\`

## Usage

1. Launch the application
2. **Taskbar Widget** (NEW!): By default, a small overlay window appears near your system tray showing:
   - ↓ Download speed (green)
   - ↑ Upload speed (orange)
   - The widget is always-on-top and semi-transparent
   - **Drag** the widget to reposition it anywhere on your screen
   - **Double-click** the widget to open the main window
3. **System Tray Icon**: The application also shows a small icon in your taskbar's notification area displaying:
   - Top text: Download speed (green)
   - Bottom text: Upload speed (orange)
4. **Hover** over the tray icon to see detailed speed information in a tooltip
5. **Double-click** the tray icon to show/hide the main window
6. **Right-click** the tray icon for options:
   - Show Window
   - Show Taskbar Widget (toggle on/off)
   - Exit
7. In the main window:
   - Select your active network interface from the dropdown
   - View real-time download and upload speeds
   - See total data transferred since application start
8. **Minimize** the window - it will hide to the system tray
9. **Close** the window - it will minimize to tray (not exit)

### Speed Display Format

**In System Tray:**
- B = Bytes per second
- K = Kilobytes per second  
- M = Megabytes per second
- G = Gigabytes per second

**In Main Window:**
- **Download Speed** (green) - Current download rate with full precision
- **Upload Speed** (yellow) - Current upload rate with full precision
- **Total Downloaded** - Cumulative data received since application start
- **Total Uploaded** - Cumulative data sent since application start

## Technical Details

- Built with C# and Windows Forms
- Uses `System.Net.NetworkInformation` for network statistics
- Updates every 1 second for accurate real-time monitoring
- Supports Ethernet, WiFi, and Gigabit Ethernet adapters

## License

Free to use and modify.
