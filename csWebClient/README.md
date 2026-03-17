# Chat Server Web Client Setup Guide

This directory contains web-based clients for connecting to csServer2 (the C# TCP chat server).

## Files Overview

- **Login.html** - Entry point for logging into the server
- **Terminal.html** - Chat/battle interface after login
- **proxy.js** - WebSocket-to-TCP bridge server (required for browser connectivity)
- **package.json** - Node.js dependencies

## Why a Proxy?

csServer2 is a **TCP socket server** that only speaks raw TCP. Browsers, however, are restricted to:
- WebSocket (for persistent connections)
- HTTP/HTTPS (for request-response communication)

The proxy translates between the two:
```
Browser (WebSocket) → Proxy (ws:// & tcp://) → csServer2 (TCP)
```

## Setup Instructions

### Prerequisites

1. **Node.js** - Download from https://nodejs.org/ (LTS recommended)
2. **csServer2** - Running on localhost:8888

### Step 1: Install Dependencies

Open PowerShell/Terminal in the `csWebClient` directory and run:

```powershell
npm install
```

This installs the `ws` (WebSocket) library required by the proxy.

### Step 2: Start the Proxy Server

In the same terminal, run:

```powershell
npm start
```

Or directly:

```powershell
node proxy.js
```

You should see:
```
[PROXY] WebSocket server listening on ws://localhost:9090
[PROXY] Forwarding connections to localhost:8888
[PROXY] Waiting for connections...
```

### Step 3: Open the Web Client

1. Open a web browser (Chrome, Firefox, Edge, Safari, etc.)
2. Navigate to: `file:///path/to/csWebClient/Login.html`
   
   OR use a local web server:
   ```powershell
   python -m http.server 8000
   # Then open http://localhost:8000/Login.html
   ```

### Step 4: Login

1. Enter your **username** (alphanumeric, max 32 characters)
2. Server address defaults to `127.0.0.1:8888`
3. Click **Login**
4. You'll be redirected to the battle arena

## How It Works

### Protocol Flow

1. **Browser connects to proxy** via WebSocket
2. **Proxy establishes TCP connection** to csServer2
3. **Username is sent** via TCP
4. **csServer2 validates** and accepts the user
5. **Messages flow** transparently both ways

### Message Format

```
Browser → Proxy: "username_here"
Proxy → csServer2: "username_here\n"

csServer2 → Proxy: "Enter Username: "
Proxy → Browser: "Enter Username: "
```

## Troubleshooting

### "Proxy Not Available" Error

**Problem**: Browser says WebSocket proxy isn't running
**Solution**: 
1. Make sure `npm install` succeeded
2. Make sure `npm start` is running in a terminal
3. Check that you're accessing the page from `file://` or `http://localhost`

### Can't Connect to Server

**Problem**: Proxy connects but can't reach csServer2
**Solution**:
1. Verify csServer2 is running on port 8888
2. In proxy.js, check `TCP_SERVER_PORT = 8888`
3. In Login.html, verify default IP is `127.0.0.1`
4. Try connecting from csClient (C# application) to verify server is working

### Username Already in Use

**Problem**: "Username already in use. Connection denied."
**Solution**: 
1. Choose a different username
2. Or disconnect the previous connection from csClient or another web client
3. Wait a few moments for the previous session to timeout

### Invalid Username Error

**Problem**: "Invalid username. Please use alphanumeric characters only."
**Solution**: 
1. Use only letters and numbers
2. Avoid special characters like `<>:"\|?*` 
3. Don't use HTTP protocol names like "GET", "HTTP", "Upgrade"
4. Keep it under 32 characters

## Features

### Login Interface
- Clean, modern UI matching the battle theme
- Real-time connection status feedback
- Input validation before attempting connection
- Secure username restrictions matching server requirements

### Battle Arena Interface
- Real-time battle mechanics
- Enemy encounters with randomized difficulty
- Four combat actions: Attack, Defend, Special Move, Flee
- Player stats dashboard
- Live battle log with color-coded events

### Server Integration
- Direct TCP communication via WebSocket proxy
- Full message routing between browser and server
- Proper connection cleanup on disconnect
- Support for server commands and responses

## Advanced Configuration

### Change Server Address

Edit **proxy.js**:
```javascript
const TCP_SERVER_HOST = 'localhost'; // Change here
const TCP_SERVER_PORT = 8888;         // Or here
```

### Change Proxy Port

Edit **proxy.js**:
```javascript
const PROXY_PORT = 9090; // Change here
```

And update **Login.html**:
```javascript
const CONFIG = {
    proxyUrl: 'ws://localhost:9090', // Match PROXY_PORT
    // ...
};
```

## Limitations

1. **Single Proxy Instance** - Each proxy handles connections sequentially
2. **No Encryption** - Uses plain WebSocket (ws://) not secure WebSocket (wss://)
3. **No Authentication** - Proxy trusts all connections, relies on server-side validation
4. **Local Only by Default** - Configured for localhost only

For production use, consider:
- Running proxy on a dedicated machine
- Using WSS (secure WebSocket) with certificates
- Implementing proxy-level authentication
- Using a proper reverse proxy (nginx with WebSocket support)

## Network Diagram

```
┌─────────────────────────────────────────────┐
│         Developer Machine                   │
├─────────────────────────────────────────────┤
│                                             │
│  Browser           Proxy          csServer2│
│  ┌──────┐          ┌──────┐      ┌────────┐
│  │ WS   │─────────→│ ws   │─────→│ TCP    │
│  │ 9090 │←─────────│ 8888 │←─────│ 8888   │
│  │      │ WebSocket│      │ TCP  │        │
│  └──────┘          └──────┘      └────────┘
│                                             │
│  http://localhost:3000/Login.html           │
│                                             │
└─────────────────────────────────────────────┘
```

## Support

For issues or questions about the web client:

1. Check that csServer2 is running
2. Verify the proxy is running: `npm start`
3. Check browser console (F12) for error messages
4. Ensure firewall isn't blocking localhost connections

## Files Reference

| File | Purpose |
|------|---------|
| `Login.html` | Login page with server connection UI |
| `Terminal.html` | Main chat/battle interface |
| `proxy.js` | WebSocket-to-TCP bridge server |
| `package.json` | Node.js project configuration |
| `README.md` | This file |

---

**Created**: For csServer2 compatibility
**Compatible with**: csServer2, csClient, web browsers
**Requirements**: Node.js with ws library
