"""
Execution layer for running helm/kubectl commands and streaming output.
"""
import subprocess
from typing import List, Optional
from orbit.core.logging import log

class ExecutionResult:
    def __init__(self, success: bool, output: str, error: Optional[str] = None):
        self.success = success
        self.output = output
        self.error = error

def run_command(cmd: List[str], dry_run: bool = False) -> ExecutionResult:
    log(f"$ {' '.join(cmd)}", color="cyan")
    if dry_run:
        log("[DRY RUN] Command not executed.", color="yellow")
        return ExecutionResult(True, "[DRY RUN]", None)
    try:
        proc = subprocess.Popen(cmd, stdout=subprocess.PIPE, stderr=subprocess.STDOUT, text=True)
        output = ""
        for line in proc.stdout:
            print(line, end="")
            output += line
        proc.wait()
        success = proc.returncode == 0
        return ExecutionResult(success, output, None if success else output)
    except Exception as e:
        log(f"Execution failed: {e}", level="error", color="red")
        return ExecutionResult(False, "", str(e))
