""" --------------------------------------------------------------------
Imports
-------------------------------------------------------------------- """
import os
import shutil
import sys
import zipfile

from datetime import datetime, timedelta
from functools import cached_property
from generate_virtual_environment import VirtualEnvironmentManager


class FnaUpdator:
    """Manages and updates the FNA game development framework.

    This class handles installing, updating, and setting up the environment for FNA.  It includes
    functionality for cloning the FNA repository, installing necessary libraries, and managing
    virtual environments.
    
    Based on https://fna-xna.github.io/docs/1%3A-Setting-Up-FNA/#chapter-3-download-and-update-fna
    """

    FNA_REPO: str = "https://github.com/FNA-XNA/FNA"
    """str: The URL for the FNA repository."""

    FNA_LIBS_REPO_FORMAT: str = "https://{pre}github.com/{post}FNA-XNA/fnalibs-dailies"
    """str: The URL for the FNA libs repository."""

    FNA_LIBS_REPO_WORKFLOW_NAME: str = "ci.yml"
    """str: The name of the workflow for the FNA libs repository."""

    def __init__(self, directory: str, personal_access_token: str) -> None:
        """Initializes the FNAUpdater.
        
        Sets up the necessary directories and installs the virtualenv package.  It also sets up the virtual environment
        if one doesn't already exist.
        
        Args:
            directory (str): The base directory for FNA installation and related files.
        """
        self._personal_access_token = personal_access_token
        self._base_directory = directory
        self._fna_repo_install_path = os.path.join(directory, "FNA")
        self._fna_libs_install_path = os.path.join(directory, "fnalibs")
        self._fna_libs_install_cache_path = os.path.join(directory, "fnalibs_cache")

        self._fna_libs_repo = self.FNA_LIBS_REPO_FORMAT.format(pre="", post="")
        self._fna_libs_repo_api = self.FNA_LIBS_REPO_FORMAT.format(pre="api.", post="repos/")

        self._manage_directory(directory=self._base_directory, delete_directory_if_exists=False,
                               create_directory_if_not_exists=True)
        
        env_manager = VirtualEnvironmentManager(directory=self._base_directory)
        env_manager.setup_environment()
        env_manager.install_package("GitPython")
        env_manager.install_package("requests")

    def execute(self) -> None:
        """Executes the FNA update or installation process.
        
        Based on the provided mode, either updates or installs FNA, then installs the required FNA libraries.
        
        Args:
            mode (str): The mode of operation, either "update" or "install".
        """
        # Step 1: Install or update the FNA repo
        self._clone_or_update_repo(
            repo=self.FNA_REPO, directory=self._fna_repo_install_path, clone_multi_options=["--recursive"]
        )

        # Step 2: Install the FNA libs
        self._install_fna_libs_manager()

        # Step 3: ???
        # Step 4: Done!
        print("Done!")
        sys.exit(0)

    @cached_property
    def _get_request_headers(self) -> dict[str, str]:
        """Gets the request headers for the GitHub API calls."""
        return {
            "Accept": "application/vnd.github.v3+json",
            "Authorization": f"Bearer {self._personal_access_token}",
            "X-GitHub-Api-Version": "2022-11-28",
        }
    
    def _manage_directory(
            self, *, directory: str, delete_directory_if_exists: bool, create_directory_if_not_exists: bool
    ) -> None:
        """Manages a directory, ensuring it exists and is a directory, not a file.
        
        Args:
            directory (str): The path to the directory to manage.
            create_directory_if_not_exists (bool): Whether to create the directory if it doesn't exist.
        """
        if os.path.exists(directory):
            if os.path.isfile(directory):
                raise Exception(f"Directory {directory} already exists and is a file?")

            if delete_directory_if_exists:
                self._remove_file_system_entry(directory)

        if create_directory_if_not_exists and not os.path.exists(directory):
            os.makedirs(directory)

    @staticmethod
    def _remove_file_system_entry(path: str) -> None:
        """Removes a file or directory from the file system.
        
        Deletes the specified file or directory.  If the path does not exist, no action is taken.
                
        Args:
            path (str): The path to the file or directory to remove.
        """
        if not os.path.exists(path):
            return

        if os.path.isfile(path):
            os.remove(path)
        else:
            shutil.rmtree(path)

    def _clone_or_update_repo(self, repo: str, directory: str, clone_multi_options: list[str]) -> None:
        """Clones or updates a Git repository with specified options.
        
        Handles repository management by either cloning a new repository or updating an existing one, including submodule initialization.
        
        Args:
            repo: The URL of the Git repository to clone or update.
            directory: The local directory path where the repository will be cloned or updated.
            clone_multi_options: Additional options to use during repository cloning.
        
        Returns:
            None
        
        Raises:
            Exception: If the specified directory is a file instead of a directory.
        """
        from git import Repo

        # Step 1: Check if we need to clone or update the repo...
        clone = True
        update = False

        repoName = os.path.basename(repo)

        if os.path.exists(directory):
            if os.path.isfile(directory):
                raise Exception(f"Directory {directory} already exists and is a file?")
            if os.path.exists(os.path.join(directory, ".git")):
                update = True
                clone = False

        # Step 2: Clone the repo?
        if clone:
            try:
                print(f"Cloning {repoName}...")
                Repo.clone_from(repo, directory, multi_options=clone_multi_options)
            except Exception:
                print("  Repo already exists, attempting to update...")
                update = True

        # Step 3: Update the repo?
        if update:
            print(f"  Updating {repoName}...")
            repo = Repo(directory)
            repo.remotes.origin.pull()

        # Step 4: Update the submodules!
        print("  Updating submodules...")
        repo = Repo(directory)
        for submodule in repo.submodules:
            submodule.update(init=True, recursive=True)

        print("  Done!")

    def _install_fna_libs_manager(self) -> None:
        """Manages the installation of FNA libraries.
        
        Downloads and extracts the FNA libraries for all supported platforms.
        """
        # Check if directory already exists
        self._manage_directory(
            directory=self._fna_libs_install_cache_path, delete_directory_if_exists=True,
            create_directory_if_not_exists=True
        )
        self._manage_directory(
            directory=self._fna_libs_install_path, delete_directory_if_exists=True, create_directory_if_not_exists=True
        )

        # get the latest run for the fnalibs workflow
        print("Determining latest fnalibs workflow run...")
        run_id = self._get_latest_run_for_workflow()
        print(f"  Run ID: {run_id}")

        # get the artifacts for the workflow run
        print("Getting artifacts for workflow run...")
        artifacts = self._get_artifacts_for_workflow_run(run_id)
        print(f"  Artifacts: {artifacts}")

        # Download all artifacts and extract them to the common location
        print("Downloading and extracting artifacts...")
        for i, (artifact_name, artifact_download_url) in enumerate(artifacts):
            self._download_and_extract_artifact(artifact_name, artifact_download_url, i + 1, len(artifacts))

        # Clean up the cache directory
        self._remove_file_system_entry(self._fna_libs_install_cache_path)

    def _get_latest_run_for_workflow(self) -> str:
        """Retrieves the latest completed workflow run for a specific GitHub Actions workflow.
        
        This method checks the latest workflow runs for today and yesterday, returning the ID of the most recent
        completed run.
        
        Returns:
            str: The ID of the latest completed workflow run, or an empty string if no run is found.
        """
        import requests

        # dates to check - today and yesterday
        dates_to_check = [
            datetime.now().date().isoformat(),
            (datetime.now().date() - timedelta(days=1)).isoformat(),
        ]
        max_i = len(dates_to_check) - 1

        base_url = f"{self._fna_libs_repo_api}/actions/workflows/{self.FNA_LIBS_REPO_WORKFLOW_NAME}/runs?per_page=1&status=completed"
        for i, day in enumerate(dates_to_check):
            day_url = f"{base_url}&created={day}"
            response = requests.get(day_url, headers=self._get_request_headers)
            data = response.json() if response.status_code == 200 else {}
            if not data or not data["workflow_runs"]:
                if i == max_i:
                    print(f"  Failed to get latest workflow run, response code: {response.status_code}")
                    sys.exit(1)
                continue
            return data["workflow_runs"][0]["id"]

    def _get_artifacts_for_workflow_run(self, run_id: str) -> list[tuple[str, str]]:
        """Retrieves artifact details for a specific GitHub Actions workflow run.
        
        Fetches the names and download URLs of artifacts associated with a given workflow run ID. 
        
        Args:
            run_id (str): The unique identifier of the workflow run.
        
        Returns:
            list[tuple[str, str]]: A list of tuples containing artifact names and their download URLs.
        
        Raises:
            SystemExit: If no artifacts are found or the request fails.
        """
        import requests

        url = f"{self._fna_libs_repo_api}/actions/runs/{run_id}/artifacts"
        response = requests.get(url, headers=self._get_request_headers)
        data = response.json() if response.status_code == 200 else {}
        if not data or not data["artifacts"]:
            print(f"  Failed to get artifacts for workflow run, response code: {response.status_code}")
            sys.exit(1)

        return [(artifact["name"], artifact["archive_download_url"]) for artifact in data["artifacts"]]

    def _download_and_extract_artifact(self, artifact_name: str, artifact_download_url: str, artifact_num: int,
                                       num_artifacts: int) -> None:
        """Downloads a specific artifact from a given URL and saves it to a local cache directory.
        
        Retrieves an artifact using a personal access token and saves it as a zip file in the specified cache location. 
        
        Args:
            artifact_name (str): The name to use for the downloaded artifact file.
            artifact_download_url (str): The URL from which to download the artifact.
        
        Returns:
            bool: True if download is successful, False otherwise.
        """
        print(f"  Downloading and extracting artifact {artifact_num}/{num_artifacts} {artifact_name}...")
        print("    Downloading artifact...")
        if not self._download_artifact(artifact_name, artifact_download_url):
            print(f"      Failed to download artifact {artifact_name}")
            sys.exit(1)
        print(f"      Artifact {artifact_name} downloaded successfully!")

        print("    Extracting artifact...")
        if not self._extract_artifact(artifact_name):
            print(f"      Failed to extract artifact {artifact_name}")
            sys.exit(1)
        print(f"      Artifact {artifact_name} extracted successfully!")

        print("    Moving artifact subdirectories...")
        for root, dirs, _ in os.walk(os.path.join(self._fna_libs_install_cache_path, artifact_name)):
            for directory in dirs:
                shutil.move(os.path.join(root, directory), self._fna_libs_install_path)
        print(f"      Artifact {artifact_name} subdirectories moved successfully!")
        print(f"  Artifact {artifact_name} downloaded and extracted successfully!")

    def _download_artifact(self, artifact_name: str, artifact_download_url: str) -> bool:
        """Downloads a specific artifact from a given URL and saves it to a local cache directory.
        
        Retrieves an artifact using a personal access token and saves it as a zip file in the specified cache location. 
        
        Args:
            artifact_name (str): The name to use for the downloaded artifact file.
            artifact_download_url (str): The URL from which to download the artifact.
        
        Returns:
            bool: True if download is successful, False otherwise.
        """
        import requests
        # Download the artifact
        response = requests.get(artifact_download_url,
                                headers={'Authorization': f'Bearer {self._personal_access_token}'})
        # Check if the request was successful
        if response.status_code == 200:
            # Open a file in write-binary mode
            with open(os.path.join(self._fna_libs_install_cache_path, f"{artifact_name}.zip"), 'wb') as file:
                # Write the content of the response to the file
                file.write(response.content)
            return True
        return False

    def _extract_artifact(self, artifact_name: str) -> bool:
        """Extracts a specific artifact from a zip file to a designated cache directory.

        Attempts to unzip an artifact file and extract its contents to a specified location. 
        
        Args:
            artifact_name (str): The name of the artifact to extract.
        
        Returns:
            bool: True if extraction is successful, False otherwise.
        """

        try:
            with zipfile.ZipFile(os.path.join(self._fna_libs_install_cache_path, f"{artifact_name}.zip"),
                                 "r") as zip_ref:
                zip_ref.extractall(os.path.join(self._fna_libs_install_cache_path, artifact_name))
        except Exception:
            return False
        return True


# Get arguments
fna_install_path = sys.argv[1].strip()  # the directory for the FNA installation
token = sys.argv[2].strip()  # the Github Personal Access Token

updator = FnaUpdator(directory=fna_install_path, personal_access_token=token)
updator.execute()
