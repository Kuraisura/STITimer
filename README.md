# STITimer

A projector-ready Windows event timer with countdown, stopwatch, speech, and quiz timing modes.

**Windows** · **Event Project** · 2026

[Portfolio](https://kuraisler.xyz)

## Purpose

STITimer gives college event organizers an audience-readable timing display. Different contest types can use their own presets, themed screens, and timing modes, replacing the need to adapt a small general-purpose stopwatch for every event.

## Intended users

Event hosts, judges, facilitators, and contestants at STI College Santa Rosa competitions and activities.

## Main capabilities

- Launch contest-specific timing displays from a preset selector.
- Run countdown and stopwatch modes with large on-screen digits.
- Use preparation/speech phases and quiz difficulty timing.
- Show themed full-screen displays suited to a projector.

## How it was built

### Windows interface

C# and WPF on .NET Framework 4.8 implement the launcher and dedicated timing windows, with XAML layouts and event branding.

### Timing modes

DispatcherTimer-based windows handle countdown, elapsed time, phased speech timing, and quiz-round timing.

### Event-focused delivery

Built in a documented 12-hour sprint for a college event, with screenshots showing the application projected during activities.

## Technology stack

| Responsibility | Technologies |
| --- | --- |
| Windows application | C#, .NET Framework 4.8, WPF, XAML |
| Timing & presentation | DispatcherTimer, Full-screen event displays |

## Project scope and current state

An event project used at STI College Santa Rosa. It is a local Windows application and does not require an online backend.

## Source guide

- [`MainWindow.xaml.cs`](MainWindow.xaml.cs)
- [`TimerWindow.xaml.cs`](TimerWindow.xaml.cs)
- [`StopwatchWindow.xaml.cs`](StopwatchWindow.xaml.cs)
- [`TrafficLightWindow.xaml.cs`](TrafficLightWindow.xaml.cs)
- [`ThinkQuestWindow.xaml.cs`](ThinkQuestWindow.xaml.cs)

## Development setup

Open STITimer.csproj in Visual Studio on Windows with the .NET desktop workload and .NET Framework 4.8 targeting pack. Build and run the application with its bundled XAML and event-art resources. This local application does not require a database or cloud account.

## Screenshots

<table>
<tr>
<td align="center"><img src="screenshots/1.png" alt="Live deployment proof" width="440"><br>Live deployment proof</td>
<td align="center"><img src="screenshots/2.png" alt="Stopwatch mode in action" width="440"><br>Stopwatch mode in action</td>
</tr>
</table>

<details>
<summary>View all 11 screenshots</summary>

### 1. Live deployment proof

![Live deployment proof](screenshots/1.png)

### 2. Stopwatch mode in action

![Stopwatch mode in action](screenshots/2.png)

### 3. Event documentation

![Event documentation](screenshots/3.png)

### 4. Venue context

![Venue context](screenshots/4.png)

### 5. Long-duration countdown

![Long-duration countdown](screenshots/5.png)

### 6. Essay contest timer

![Essay contest timer](screenshots/6.png)

### 7. Stopwatch for creative contests

![Stopwatch for creative contests](screenshots/7.png)

### 8. Preparation mode

![Preparation mode](screenshots/8.png)

### 9. Quiz difficulty timer

![Quiz difficulty timer](screenshots/9.png)

### 10. Travelogue stopwatch

![Travelogue stopwatch](screenshots/10.png)

### 11. Cooking contest countdown

![Cooking contest countdown](screenshots/11.png)

</details>

---

Project by [Mark Crysler Baddo](https://github.com/Kuraisura) · [Portfolio](https://kuraisler.xyz)
