// src/preload.ts
import { contextBridge } from "electron";

contextBridge.exposeInMainWorld("api", {
  startServer: () => console.log("Start Server"),
  stopServer: () => console.log("Stop Server"),
});
