document.addEventListener("DOMContentLoaded", function () {
    const commentsContainer = document.getElementById("comments-list");
    const form = document.getElementById("comment-form");
    const projectIdInput = document.getElementById("project-id");
    const contentInput = document.getElementById("comment-content");

    if (!commentsContainer || !form || !projectIdInput || !contentInput) {
        return;
    }

    const projectId = parseInt(projectIdInput.value);

    function renderComments(comments) {
        if (!comments || comments.length === 0) {
            commentsContainer.innerHTML = "<p class=\"text-muted\">No comments yet. Be the first to comment!</p>";
            return;
        }

        const items = comments.map(c => {
            const created = c.createdDate || c.CreatedDate || c.created || "";
            return `
                <div class="card mb-2">
                    <div class="card-body py-2">
                        <p class="mb-1">${c.content}</p>
                        <small class="text-muted">Posted on ${created}</small>
                    </div>
                </div>`;
        });

        commentsContainer.innerHTML = items.join("");
    }

    async function loadComments() {
        try {
            const response = await fetch(`/api/ProjectComment/GetComments?projectId=${projectId}`);
            if (!response.ok) {
                throw new Error("Failed to load comments");
            }
            const data = await response.json();
            renderComments(data);
        } catch (err) {
            commentsContainer.innerHTML = "<p class=\"text-danger\">Unable to load comments.</p>";
            console.error(err);
        }
    }

    form.addEventListener("submit", async function (e) {
        e.preventDefault();

        const content = contentInput.value.trim();
        if (!content) {
            return;
        }

        try {
            const response = await fetch("/api/ProjectComment/AddComment", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify({ projectId: projectId, content: content })
            });

            if (!response.ok) {
                throw new Error("Failed to add comment");
            }

            contentInput.value = "";
            await loadComments();
        } catch (err) {
            alert("There was a problem adding your comment.");
            console.error(err);
        }
    });

    loadComments();
});
