export interface FamilyMemberDto {
    userId: string;
    fullName: string;
    email: string;
    panNumber: string;
    addedAt: string;
}

export interface FamilyGroupDto {
    id: number;
    groupName: string;
    headUserId: string;
    headUserName: string;
    headUserEmail: string;
    headPanNumber: string;
    createdAt: string;
    isActive: boolean;
    members: FamilyMemberDto[];
    memberCount: number;
}

export interface CreateFamilyGroupDto {
    groupName: string;
    headUserId: string;
}

export interface AddFamilyMemberDto {
    userId: string;
}