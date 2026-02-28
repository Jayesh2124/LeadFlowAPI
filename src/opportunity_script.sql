START TRANSACTION;
CREATE TABLE opportunities (
    id uuid NOT NULL,
    lead_id uuid NOT NULL,
    created_by_user_id uuid NOT NULL,
    owner_user_id uuid NOT NULL,
    title character varying(200) NOT NULL,
    description character varying(2000),
    type character varying(50) NOT NULL,
    status character varying(50) NOT NULL,
    priority character varying(50) NOT NULL,
    expected_value numeric(18,2) NOT NULL,
    expected_start_date timestamp with time zone,
    expected_end_date timestamp with time zone,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone NOT NULL,
    CONSTRAINT "PK_opportunities" PRIMARY KEY (id),
    CONSTRAINT "FK_opportunities_leads_lead_id" FOREIGN KEY (lead_id) REFERENCES leads ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_opportunities_users_created_by_user_id" FOREIGN KEY (created_by_user_id) REFERENCES users ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_opportunities_users_owner_user_id" FOREIGN KEY (owner_user_id) REFERENCES users ("Id") ON DELETE RESTRICT
);

CREATE TABLE opportunity_documents (
    id uuid NOT NULL,
    opportunity_id uuid NOT NULL,
    file_name character varying(255) NOT NULL,
    file_url character varying(1000) NOT NULL,
    document_type character varying(50) NOT NULL,
    uploaded_by_user_id uuid NOT NULL,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone NOT NULL,
    CONSTRAINT "PK_opportunity_documents" PRIMARY KEY (id),
    CONSTRAINT "FK_opportunity_documents_opportunities_opportunity_id" FOREIGN KEY (opportunity_id) REFERENCES opportunities (id) ON DELETE CASCADE,
    CONSTRAINT "FK_opportunity_documents_users_uploaded_by_user_id" FOREIGN KEY (uploaded_by_user_id) REFERENCES users ("Id") ON DELETE RESTRICT
);

CREATE INDEX "IX_opportunities_created_by_user_id" ON opportunities (created_by_user_id);

CREATE INDEX "IX_opportunities_lead_id" ON opportunities (lead_id);

CREATE INDEX "IX_opportunities_owner_user_id" ON opportunities (owner_user_id);

CREATE INDEX "IX_opportunities_status" ON opportunities (status);

CREATE INDEX "IX_opportunity_documents_opportunity_id" ON opportunity_documents (opportunity_id);

CREATE INDEX "IX_opportunity_documents_uploaded_by_user_id" ON opportunity_documents (uploaded_by_user_id);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260227130359_AddOpportunityFoundation', '9.0.2');

COMMIT;

