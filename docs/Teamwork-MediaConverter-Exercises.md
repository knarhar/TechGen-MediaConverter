# Process + Threading Exercise (Mock Converter)

## Goal
Build a console "conversion manager" that launches a separate mock worker process per job, tracks progress, and supports cancellation.

## Functional Requirements

### 1) Interactive mode only
- Menu navigation for:
  - add job
  - monitor progress
  - cancel one/all
  - list/wait/exit

### 2) Job model
- Each job has:
  - input path (or name),
  - output path (or name),
  - options text,
  - status (`Queued`, `Running`, `Completed`, `Failed`, `Canceled`),
  - progress percent.

### 3) Worker process
- Do not perform real media conversion.
- Start an external mock process that:
  - receives input/output/options arguments,
  - emits progress lines over time,
  - completes with random success/failure.

### 4) Progress handling
- Parse worker output and update job progress.
- Show live progress in interactive monitor screen.

### 5) Concurrency
- Support multiple queued jobs.
- Process jobs via worker threads.
- Use synchronization primitives (`lock`, `Monitor.Wait/Pulse` or equivalent) for safe shared state.

### 6) Cancellation
- Cancel a single job.
- Cancel all queued/running jobs.
- Distinguish behavior:
  - queued cancel (never started),
  - running cancel (terminate worker process).

## Suggested Tasks

1. Implement core job queue and worker loop.  
2. Add interactive menu + live monitor.  
3. Add cancellation APIs (single/all).  
4. Add a clean help screen in the menu.

