"""
Colored logging for Orbit.
"""
from typing import Optional
import sys

COLORS = {
    "reset": "\033[0m",
    "red": "\033[91m",
    "green": "\033[92m",
    "yellow": "\033[93m",
    "blue": "\033[94m",
    "cyan": "\033[96m",
}

def log(msg: str, level: str = "info", color: Optional[str] = None):
    prefix = {
        "info": "[INFO]",
        "error": "[ERROR]",
        "warn": "[WARN]",
        "debug": "[DEBUG]"
    }.get(level, "[INFO]")
    color_code = COLORS.get(color, "")
    reset = COLORS["reset"] if color_code else ""
    print(f"{color_code}{prefix} {msg}{reset}", file=sys.stderr if level == "error" else sys.stdout)

def setup_logging():
    pass  # Placeholder for future log config
