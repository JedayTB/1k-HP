import { app, BrowserWindow } from "electron";
import path from "path";

let mainWindow: BrowserWindow | null;

app.on("ready", () => {
  if (mainWindow === null) {
    mainWindow = new BrowserWindow({
      width: 1400,
      height: 800,
      //icon: "./icon.ico",
      resizable: false,
      center: true,
      fullscreenable: false,
      fullscreen: false,
      frame: false,
      autoHideMenuBar: true,
      webPreferences: {
        preload: path.join(__dirname, "preload.js"),
        contextIsolation: false,
        nodeIntegration: true,
      },
    });

    mainWindow.loadFile(path.join(__dirname, "../public/controller.html"));
  }

  mainWindow.on("closed", () => {
    mainWindow = null;
  });
});

app.on("window-all-closed", () => {
  app.quit();
});
