import os
import selectors
import subprocess
import sys

# Step 1: Get the directory path of the current script
current_file_directory = os.path.dirname(os.path.realpath(__file__))

# Step 2: Set our install path
fna_install_path = os.path.join(os.path.dirname(current_file_directory), ".fna")

# Step 3: Check if something exists in the install path. If it does, don't do anything!
if os.path.exists(fna_install_path):
    print("FNA already exists in the current directory! Exiting...")
    sys.exit(0)

# Step 4: Get the Github Personal Access Token with Actions (read) scope
token: str = ""
if os.path.exists(".gitpersonalaccesstoken") and os.path.isfile(".gitpersonalaccesstoken"):
    with open(".gitpersonalaccesstoken", "r") as file:
        token = file.read()
else:
    raise Exception("Please execute `update_or_install_fna.py`!")

# Step 5: Call fna_updator.py with the install path we want
fna_updator_path = os.path.join(current_file_directory, ".build", "fna_updator.py")
command = ["python", fna_updator_path, fna_install_path, token]

if sys.platform == "win32":
    p = subprocess.Popen(command, stdout=subprocess.PIPE, stderr=subprocess.PIPE, universal_newlines=True, shell=True)
    ok = True
    while ok:
        for line in p.stdout:
            print(f"{line}", end="")
        for line in p.stderr:
            print(f"ERROR: {line}", end="", file=sys.stderr)
        if p.poll() is not None:
            ok = False
else:
    p = subprocess.Popen(command, stdout=subprocess.PIPE, stderr=subprocess.PIPE, universal_newlines=True)
    sel = selectors.DefaultSelector()
    sel.register(p.stdout, selectors.EVENT_READ)
    sel.register(p.stderr, selectors.EVENT_READ)
    ok = True
    while ok:
        for key, val1 in sel.select():
            line = key.fileobj.readline()
            if not line:
                ok = False
                break
            if key.fileobj is p.stdout:
                print(f"{line}", end="")
            else:
                print(f"ERROR: {line}", end="", file=sys.stderr)
