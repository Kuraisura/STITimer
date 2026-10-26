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

## Project video

### STITimer screenshot walkthrough

**Screenshot walkthrough · 1:09**

A silent, captioned walkthrough of the existing event photos and timer screenshots. It explains live-event context, countdowns, stopwatch presets, speech phases, and quiz timing. This is a still-image presentation, not a recording of live timer interaction.

[![Watch STITimer screenshot walkthrough](screenshots/1.png)](https://kuraisler.xyz/Portfolio/STITimer/Showcase/screenshot-walkthrough.mp4)

[Watch in your browser](https://kuraisler.xyz/Portfolio/STITimer/Showcase/screenshot-walkthrough.mp4) · [Video file in this repository](media/screenshot-walkthrough.mp4)

## Screenshot and module walkthrough

Expand a screen below to see the capture and an explanation of the module, its visible controls, and its role in the application.

<details open>
<summary><strong>01 · All in Place live-event setup</strong></summary>

A laptop and projected display show the All in Place timer during an activity. The photograph documents the application in an event setting, with large digits visible beyond the operator screen.

<p><img src="screenshots/1.png" alt="All in Place live-event setup" width="960"></p>

</details>

<details>
<summary><strong>02 · Chef Xpress elapsed-time display</strong></summary>

A venue laptop shows the Chef Xpress preset counting elapsed time. This photograph demonstrates how an organizer can place the timing interface at the activity site.

<p><img src="screenshots/2.png" alt="Chef Xpress elapsed-time display" width="960"></p>

</details>

<details>
<summary><strong>03 · Travelogue timer at an event</strong></summary>

The Travelogue preset is visible on a laptop during a presentation setting. The elapsed-time display gives the operator a reference for tracking how long a presentation has run.

<p><img src="screenshots/3.png" alt="Travelogue timer at an event" width="960"></p>

</details>

<details>
<summary><strong>04 · Event venue context</strong></summary>

The photograph shows contestants, judges, equipment, and the THM Week venue. It documents the environment associated with the timer project, rather than exposing a separate software module.

<p><img src="screenshots/4.png" alt="Event venue context" width="960"></p>

</details>

<details>
<summary><strong>05 · Codefest long-duration countdown</strong></summary>

The Codefest preset opens at 36 hours with Start, Reset, and keyboard hints. It adapts the countdown module to a long coding competition while keeping the display readable at a distance.

<p><img src="screenshots/5.png" alt="Codefest long-duration countdown" width="960"></p>

</details>

<details>
<summary><strong>06 · Essay contest countdown</strong></summary>

The Essay Does It preset starts from one hour and shares the countdown controls. Event artwork identifies the contest while the timer gives participants a common deadline.

<p><img src="screenshots/6.png" alt="Essay contest countdown" width="960"></p>

</details>

<details>
<summary><strong>07 · Mix N Flair stopwatch</strong></summary>

The elapsed-time preset starts at zero, with Start and Reset controls. It records duration instead of counting down from a fixed limit, suiting activities measured by time taken.

<p><img src="screenshots/7.png" alt="Mix N Flair stopwatch" width="960"></p>

</details>

<details>
<summary><strong>08 · Preparation and speech phases</strong></summary>

Stand Up and Deliver shows a preparation countdown, separate prep/speech duration settings, and a skip-preparation option. This module supports moving between the two stages of a speaking activity.

<p><img src="screenshots/8.png" alt="Preparation and speech phases" width="960"></p>

</details>

<details>
<summary><strong>09 · Think Quest question timer</strong></summary>

Easy, Medium, and Hard choices expose 15-, 30-, and 45-second limits above a large countdown. The quiz timer lets the host select a duration appropriate to the current round.

<p><img src="screenshots/9.png" alt="Think Quest question timer" width="960"></p>

</details>

<details>
<summary><strong>10 · Travelogue stopwatch preset</strong></summary>

The Travelogue interface begins at zero with elapsed-time labeling and standard timer controls. It reuses the stopwatch behavior with presentation-specific branding.

<p><img src="screenshots/10.png" alt="Travelogue stopwatch preset" width="960"></p>

</details>

<details>
<summary><strong>11 · Worth the Whisk countdown</strong></summary>

The cooking/baking preset opens at three hours with Start, Reset, and keyboard shortcuts. It applies the countdown module to a longer practical contest.

<p><img src="screenshots/11.png" alt="Worth the Whisk countdown" width="960"></p>

</details>

---

Project by [Mark Crysler Baddo](https://github.com/Kuraisura) · [Portfolio](https://kuraisler.xyz)
